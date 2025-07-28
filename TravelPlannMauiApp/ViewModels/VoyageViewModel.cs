using BU.Services;
using DAL.DB;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using TravelPlannMauiApp.Pages;
using System.Diagnostics;
using System.ComponentModel;

namespace TravelPlannMauiApp.ViewModels
{
    public class VoyageViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private readonly ISessionService _sessionService;
        private bool _isLoading;
        private int _currentUserId;
        
        public ObservableCollection<VoyageItemViewModel> Voyages { get; } = new();
        public ICommand LoadVoyagesCommand { get; }
        public ICommand ViewVoyageDetailsCommand { get; }
        public ICommand AddVoyageCommand { get; }
        public ICommand ToggleCompleteVoyageCommand { get; }
        public ICommand ToggleArchiveVoyageCommand { get; }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        public VoyageViewModel(IVoyageService voyageService, ISessionService sessionService = null)
        {
            _voyageService = voyageService;
            _sessionService = sessionService;

            LoadVoyagesCommand = new Command(async () => await LoadVoyagesAsync());
            ViewVoyageDetailsCommand = new Command<VoyageItemViewModel>(async (v) => await ViewVoyageDetails(v));
            AddVoyageCommand = new Command(async () => await AddVoyageAsync());
            ToggleCompleteVoyageCommand = new Command<VoyageItemViewModel>(async (v) => await ToggleCompleteVoyage(v));
            ToggleArchiveVoyageCommand = new Command<VoyageItemViewModel>(async (v) => await ToggleArchiveVoyage(v));
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                int userId = 0;
                
                if (_sessionService != null)
                {
                    var userIdNullable = await _sessionService.GetCurrentUserIdAsync();
                    userId = userIdNullable ?? 0;
                }
                else
                {
                    userId = await GetCurrentUserIdWithFallback();
                }

                if (userId > 0)
                {
                    Debug.WriteLine($"ID utilisateur récupéré: {userId}");
                    return userId;
                }
                else
                {
                    Debug.WriteLine("Aucun ID utilisateur trouvé - redirection vers login");
                    await Shell.Current.DisplayAlert("Session expirée", 
                        "Votre session a expiré. Vous allez être redirigé vers la page de connexion.", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la récupération de l'ID utilisateur: {ex}");
                await Shell.Current.DisplayAlert("Erreur", 
                    "Impossible de récupérer les informations utilisateur.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
                return 0;
            }
        }

        private async Task<int> GetCurrentUserIdWithFallback()
        {
            try
            {
                var userIdString = await SecureStorage.GetAsync("current_user_id");
                if (int.TryParse(userIdString, out int userId))
                {
                    Debug.WriteLine($"ID utilisateur trouvé dans SecureStorage: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur SecureStorage pour ID utilisateur: {ex.Message}");
            }

            try
            {
                var userIdString = Preferences.Get("current_user_id", null);
                if (int.TryParse(userIdString, out int userId))
                {
                    Debug.WriteLine($"ID utilisateur trouvé dans Preferences: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur Preferences pour ID utilisateur: {ex.Message}");
            }

            Debug.WriteLine("Aucun ID utilisateur trouvé dans le stockage");
            return 0;
        }
     
        public async Task LoadVoyagesAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                Debug.WriteLine("=== DÉBUT CHARGEMENT VOYAGES ===");
                
                _currentUserId = await GetCurrentUserIdAsync();
                if (_currentUserId == 0) 
                {
                    Debug.WriteLine("Utilisateur non connecté - arrêt du chargement");
                    return;
                }

                Debug.WriteLine($"Chargement des voyages pour l'utilisateur ID: {_currentUserId}");
                
                await MainThread.InvokeOnMainThreadAsync(() => Voyages.Clear());
                
                var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(_currentUserId);
                
                Debug.WriteLine($"Nombre de voyages trouvés: {voyages?.Count ?? 0}");
                
                if (voyages != null && voyages.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (var v in voyages)
                        {
                            Debug.WriteLine($"Ajout du voyage: {v.NomVoyage} (ID: {v.VoyageId}) - Complete: {v.EstComplete}, Archive: {v.EstArchive}");
                            var voyageItemViewModel = new VoyageItemViewModel(v);
                            Voyages.Add(voyageItemViewModel);
                        }
                    });
                }
                else
                {
                    Debug.WriteLine("Aucun voyage retourné par le service");
                }
                
                Debug.WriteLine($"Total voyages dans la collection: {Voyages.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== ERREUR CHARGEMENT VOYAGES ===");
                Debug.WriteLine($"Exception: {ex}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Erreur", 
                        $"Erreur lors du chargement des voyages: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("=== FIN CHARGEMENT VOYAGES ===");
            }
        }
      
        private async Task ToggleCompleteVoyage(VoyageItemViewModel voyageViewModel)
        {
            if (voyageViewModel == null) return;

            try
            {
                var voyage = voyageViewModel.Voyage;
                
                if (voyage.UtilisateurId != _currentUserId)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Vous n'êtes pas autorisé à modifier ce voyage", "OK");
                    return;
                }

                // CORRECTION 1: Récupérer le voyage complet avec ses relations
                var voyageDetails = await _voyageService.GetVoyageDetailsAsync(voyage.VoyageId);
                if (voyageDetails?.Voyage == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Voyage introuvable", "OK");
                    return;
                }

                var voyageToUpdate = voyageDetails.Voyage;
                voyageToUpdate.EstComplete = !voyageToUpdate.EstComplete;

                if (voyageToUpdate.EstComplete) 
                {
                    voyageToUpdate.EstArchive = false;
                }

                // CORRECTION 2: Passer le voyage complet avec ses relations
                await _voyageService.UpdateVoyageAsync(voyageToUpdate);
                
                // CORRECTION 3: Mise à jour immédiate de l'UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    voyageViewModel.UpdateFromVoyage(voyageToUpdate);
                    // Forcer une mise à jour de l'affichage
                    OnPropertyChanged(nameof(Voyages));
                });
                
                Debug.WriteLine($"Voyage {voyage.NomVoyage} - EstComplete: {voyageToUpdate.EstComplete}, EstArchive: {voyageToUpdate.EstArchive}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la modification du statut: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Erreur", 
                        $"Erreur lors de la modification du statut: {ex.Message}", "OK");
                });
            }
        }
       
        private async Task ToggleArchiveVoyage(VoyageItemViewModel voyageViewModel)
        {
            if (voyageViewModel == null) return;

            try
            {
                var voyage = voyageViewModel.Voyage;
                
                if (voyage.UtilisateurId != _currentUserId)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Vous n'êtes pas autorisé à modifier ce voyage", "OK");
                    return;
                }

                if (!voyage.EstArchive && !voyage.EstComplete)
                {
                    bool confirm = await Shell.Current.DisplayAlert(
                        "Confirmation",
                        "Archiver ce voyage non complété ?",
                        "Oui", "Non");
                    if (!confirm) return;
                }

                // CORRECTION 1: Récupérer le voyage complet avec ses relations
                var voyageDetails = await _voyageService.GetVoyageDetailsAsync(voyage.VoyageId);
                if (voyageDetails?.Voyage == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Voyage introuvable", "OK");
                    return;
                }

                var voyageToUpdate = voyageDetails.Voyage;
                voyageToUpdate.EstArchive = !voyageToUpdate.EstArchive;
                
                // CORRECTION 2: Passer le voyage complet avec ses relations
                await _voyageService.UpdateVoyageAsync(voyageToUpdate);
                
                // CORRECTION 3: Mise à jour immédiate de l'UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    voyageViewModel.UpdateFromVoyage(voyageToUpdate);
                    // Forcer une mise à jour de l'affichage
                    OnPropertyChanged(nameof(Voyages));
                });
                
                Debug.WriteLine($"Voyage {voyage.NomVoyage} - EstComplete: {voyageToUpdate.EstComplete}, EstArchive: {voyageToUpdate.EstArchive}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'archivage: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Erreur", 
                        $"Erreur lors de l'archivage: {ex.Message}", "OK");
                });
            }
        }
        
        public async Task ViewVoyageDetails(VoyageItemViewModel voyageViewModel)
        {
            if (voyageViewModel == null) return;

            try
            {
                var voyage = voyageViewModel.Voyage;
                
                if (voyage.UtilisateurId != _currentUserId)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Vous n'êtes pas autorisé à voir ce voyage", "OK");
                    return;
                }

                Debug.WriteLine($"Navigation vers détails du voyage ID: {voyage.VoyageId}");
                
                var voyageDetails = await _voyageService.GetVoyageDetailsAsync(voyage.VoyageId);
                if (voyageDetails?.Voyage == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Voyage introuvable", "OK");
                    return;
                }
                
                var voyageComplet = voyageDetails.Voyage;
                
                var activitesDto = voyageDetails.Activites?.Select(a => new ActiviteDTO
                {
                    ActiviteId = a.ActiviteId,
                    Nom = a.Nom,
                    Description = a.Description,
                    Localisation = a.Localisation
                }).ToList() ?? new List<ActiviteDTO>();
                
                var hebergementsDto = voyageDetails.Hebergements?.Select(h => new HebergementDTO
                {
                    HebergementId = h.HebergementId,
                    Nom = h.Nom,
                    TypeHebergement = h.TypeHebergement,
                    Cout = h.Cout,
                    DateDebut = h.DateDebut,
                    DateFin = h.DateFin,
                    Adresse = h.Adresse
                }).ToList() ?? new List<HebergementDTO>();

                var dto = new VoyageDetailsDTO
                {
                    VoyageID = voyageComplet.VoyageId,
                    NomVoyage = voyageComplet.NomVoyage,
                    Description = voyageComplet.Description,
                    DateDebut = voyageComplet.DateDebut.ToDateTime(TimeOnly.MinValue),
                    DateFin = voyageComplet.DateFin.ToDateTime(TimeOnly.MinValue),
                    EstComplete = voyageComplet.EstComplete,
                    EstArchive = voyageComplet.EstArchive,
                    UtilisateurId = voyageComplet.UtilisateurId,
                    Activites = activitesDto,
                    Hebergements = hebergementsDto
                };

                var serialized = JsonSerializer.Serialize(dto);
                Debug.WriteLine($"Données sérialisées: {serialized}");

                await Shell.Current.GoToAsync($"{nameof(VoyageDetailsPage)}?ViewModel={Uri.EscapeDataString(serialized)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur navigation: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Erreur", 
                        $"Erreur lors de la navigation: {ex.Message}", "OK");
                });
            }
        }

        public async Task RefreshVoyagesAsync()
        {
            await LoadVoyagesAsync();
        }
        
        public async Task AddVoyageAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddVoyagePage));
        }
    }

    // CORRECTION 4: Amélioration du VoyageItemViewModel
    public class VoyageItemViewModel : INotifyPropertyChanged
    {
        private Voyage _voyage;
        private bool _estComplete;
        private bool _estArchive;

        public VoyageItemViewModel(Voyage voyage)
        {
            _voyage = voyage ?? throw new ArgumentNullException(nameof(voyage));
            _estComplete = voyage.EstComplete;
            _estArchive = voyage.EstArchive;
        }

        public Voyage Voyage => _voyage;

        public int VoyageId => _voyage.VoyageId;
        public string NomVoyage => _voyage.NomVoyage;
        public string Description => _voyage.Description;
        public DateOnly DateDebut => _voyage.DateDebut;
        public DateOnly DateFin => _voyage.DateFin;
        public int UtilisateurId => _voyage.UtilisateurId;

        public bool EstComplete 
        { 
            get => _estComplete;
            private set
            {
                if (_estComplete != value)
                {
                    _estComplete = value;
                    _voyage.EstComplete = value;
                    OnPropertyChanged();
                    // Forcer la mise à jour de l'affichage en déclenchant tous les changements liés
                    OnPropertyChanged(nameof(EstArchive)); // Pour s'assurer que les indicateurs se mettent à jour
                }
            }
        }

        public bool EstArchive 
        { 
            get => _estArchive;
            private set
            {
                if (_estArchive != value)
                {
                    _estArchive = value;
                    _voyage.EstArchive = value;
                    OnPropertyChanged();
                    // Forcer la mise à jour de l'affichage en déclenchant tous les changements liés
                    OnPropertyChanged(nameof(EstComplete)); // Pour s'assurer que les indicateurs se mettent à jour
                }
            }
        }

        public void UpdateFromVoyage(Voyage updatedVoyage)
        {
            // Mise à jour avec notification des changements
            EstComplete = updatedVoyage.EstComplete;
            EstArchive = updatedVoyage.EstArchive;
            
            // Déclencher une mise à jour complète pour s'assurer que l'UI se rafraîchit
            OnPropertyChanged(string.Empty); // Met à jour toutes les propriétés
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // DTOs sans cycles de référence pour la sérialisation
    public class VoyageDetailsDTO
    {
        public int VoyageID { get; set; }
        public string NomVoyage { get; set; }
        public string Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public bool EstComplete { get; set; }
        public bool EstArchive { get; set; }
        public int UtilisateurId { get; set; }
        public List<ActiviteDTO> Activites { get; set; } = new();
        public List<HebergementDTO> Hebergements { get; set; } = new();
    }
    
    public class ActiviteDTO
    {
        public int ActiviteId { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public string Localisation { get; set; }
    }
    
    public class HebergementDTO
    {
        public int HebergementId { get; set; }
        public string Nom { get; set; }
        public string TypeHebergement { get; set; }
        public decimal? Cout { get; set; }
        public DateOnly? DateDebut { get; set; }
        public DateOnly? DateFin { get; set; }
        public string Adresse { get; set; }
    }
}
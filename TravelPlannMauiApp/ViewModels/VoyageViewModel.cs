using BU.Services;
using DAL.DB;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using TravelPlannMauiApp.Pages;
using System.Diagnostics;

namespace TravelPlannMauiApp.ViewModels
{
    public class VoyageViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private readonly ISessionService _sessionService;
        private bool _isBusy;
        private int _currentUserId;
        
        public ObservableCollection<Voyage> Voyages { get; } = new();
        public ICommand LoadVoyagesCommand { get; }
        public ICommand ViewVoyageDetailsCommand { get; }
        public ICommand AddVoyageCommand { get; }
        public ICommand ToggleCompleteVoyageCommand { get; }
        public ICommand ToggleArchiveVoyageCommand { get; }
        
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        
        public VoyageViewModel(IVoyageService voyageService, ISessionService sessionService = null)
        {
            _voyageService = voyageService;
            _sessionService = sessionService;

            LoadVoyagesCommand = new Command(async () => await LoadVoyagesAsync());
            ViewVoyageDetailsCommand = new Command<Voyage>(async (v) => await ViewVoyageDetails(v));
            AddVoyageCommand = new Command(async () => await AddVoyageAsync());
            ToggleCompleteVoyageCommand = new Command<Voyage>(async (v) => await ToggleCompleteVoyage(v));
            ToggleArchiveVoyageCommand = new Command<Voyage>(async (v) => await ToggleArchiveVoyage(v));
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                int userId = 0;
                
                if (_sessionService != null)
                {
                    // Utiliser le service de session si disponible
                    var userIdNullable = await _sessionService.GetCurrentUserIdAsync();
                    userId = userIdNullable ?? 0;
                }
                else
                {
                    // Fallback direct sur SecureStorage/Preferences
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
                // Essayer SecureStorage d'abord
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
                // Fallback vers Preferences
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
            if (IsBusy) return;

            IsBusy = true;
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
                
                // Utiliser Dispatcher pour s'assurer que les modifications UI se font sur le bon thread
                await MainThread.InvokeOnMainThreadAsync(() => Voyages.Clear());
                
                // Charger uniquement les voyages de l'utilisateur connecté
                var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(_currentUserId);
                
                Debug.WriteLine($"Nombre de voyages trouvés: {voyages?.Count ?? 0}");
                
                if (voyages != null && voyages.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (var v in voyages)
                        {
                            Debug.WriteLine($"Ajout du voyage: {v.NomVoyage} (ID: {v.VoyageId}) - Complete: {v.EstComplete}, Archive: {v.EstArchive}");
                            Voyages.Add(v);
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
                IsBusy = false;
                Debug.WriteLine("=== FIN CHARGEMENT VOYAGES ===");
            }
        }
      
        private async Task ToggleCompleteVoyage(Voyage voyage)
        {
            if (voyage == null) return;

            try
            {
                // Vérifier que l'utilisateur est propriétaire du voyage
                if (voyage.UtilisateurId != _currentUserId)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Vous n'êtes pas autorisé à modifier ce voyage", "OK");
                    return;
                }

                // Créer une copie propre du voyage pour la mise à jour
                var voyageToUpdate = new Voyage
                {
                    VoyageId = voyage.VoyageId,
                    NomVoyage = voyage.NomVoyage,
                    Description = voyage.Description,
                    DateDebut = voyage.DateDebut,
                    DateFin = voyage.DateFin,
                    UtilisateurId = voyage.UtilisateurId,
                    EstComplete = !voyage.EstComplete, // Inverser l'état
                    EstArchive = voyage.EstArchive
                };

                // Si on marque comme complet, on désarchive automatiquement
                if (voyageToUpdate.EstComplete) 
                {
                    voyageToUpdate.EstArchive = false;
                }

                // Sauvegarder les changements
                await _voyageService.UpdateVoyageAsync(voyageToUpdate);
                
                // Mettre à jour l'objet dans la collection ObservableCollection
                voyage.EstComplete = voyageToUpdate.EstComplete;
                voyage.EstArchive = voyageToUpdate.EstArchive;
                
                Debug.WriteLine($"Voyage {voyage.NomVoyage} - EstComplete: {voyage.EstComplete}, EstArchive: {voyage.EstArchive}");

                // Forcer la mise à jour de l'interface utilisateur
                OnPropertyChanged(nameof(Voyages));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la modification du statut: {ex}");
                await HandleError(ex, "Erreur lors de la modification du statut");
            }
        }
       
        private async Task ToggleArchiveVoyage(Voyage voyage)
        {
            if (voyage == null) return;

            try
            {
                // Vérifier que l'utilisateur est propriétaire du voyage
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

                // Créer une copie propre du voyage pour la mise à jour
                var voyageToUpdate = new Voyage
                {
                    VoyageId = voyage.VoyageId,
                    NomVoyage = voyage.NomVoyage,
                    Description = voyage.Description,
                    DateDebut = voyage.DateDebut,
                    DateFin = voyage.DateFin,
                    UtilisateurId = voyage.UtilisateurId,
                    EstComplete = voyage.EstComplete,
                    EstArchive = !voyage.EstArchive // Inverser l'état
                };
                
                // Sauvegarder les changements
                await _voyageService.UpdateVoyageAsync(voyageToUpdate);
                
                // Mettre à jour l'objet dans la collection ObservableCollection
                voyage.EstArchive = voyageToUpdate.EstArchive;
                
                Debug.WriteLine($"Voyage {voyage.NomVoyage} - EstComplete: {voyage.EstComplete}, EstArchive: {voyage.EstArchive}");

                // Forcer la mise à jour de l'interface utilisateur
                OnPropertyChanged(nameof(Voyages));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'archivage: {ex}");
                await HandleError(ex, "Erreur lors de l'archivage");
            }
        }
        
        public async Task ViewVoyageDetails(Voyage voyage)
        {
            if (voyage == null) return;

            try
            {
                // Vérifier que l'utilisateur est propriétaire du voyage
                if (voyage.UtilisateurId != _currentUserId)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Vous n'êtes pas autorisé à voir ce voyage", "OK");
                    return;
                }

                Debug.WriteLine($"Navigation vers détails du voyage ID: {voyage.VoyageId}");
                
                // Récupérer les détails complets du voyage
                var voyageDetails = await _voyageService.GetVoyageDetailsAsync(voyage.VoyageId);
                if (voyageDetails?.Voyage == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Voyage introuvable", "OK");
                    return;
                }
                
                var voyageComplet = voyageDetails.Voyage;
                
                // CORRECTION: Créer des DTOs sans cycles de référence
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
                    // Utiliser les DTOs sans cycles de référence
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
                await HandleError(ex, "Erreur lors de la navigation");
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
using BU.Services;
using DAL.DB;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TravelPlannMauiApp.ViewModels
{
    public class VoyageDetailsViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private readonly IActiviteService _activiteService;
        private readonly IHebergementService _hebergementService;

        private int _voyageId;
        private bool _isViewMode = true;
        private bool _isEditMode;
        private bool _isLoading = false;
        
        // Propriétés du voyage
        private string _nomVoyage;
        private string _description;
        private DateOnly _dateDebut = DateOnly.FromDateTime(DateTime.Today);
        private DateOnly _dateFin = DateOnly.FromDateTime(DateTime.Today);
        private bool _estComplete;
        private bool _estArchive;
        private int _utilisateurId;
        
        // Propriétés originales pour annulation
        private string _originalNomVoyage;
        private string _originalDescription;
        private DateOnly _originalDateDebut;
        private DateOnly _originalDateFin;
        private List<Activite> _originalActivites;
        private List<Hebergement> _originalHebergements;
        
        // Propriétés pour les formulaires d'activités
        private bool _showActiviteForm;
        private string _nouvelleActiviteNom;
        private string _nouvelleActiviteDescription;
        private string _nouvelleActiviteLocalisation; // NOUVEAU

        // Propriétés pour les formulaires d'hébergements
        private bool _showHebergementForm;
        private string _nouvelHebergementNom;
        private string _nouvelHebergementType;
        private string _nouvelHebergementAdresse; // NOUVEAU
        private decimal _nouvelHebergementCout;

        // Collections
        public ObservableCollection<Activite> Activites { get; } = new();
        public ObservableCollection<Hebergement> Hebergements { get; } = new();

        // Commandes
        public ICommand ToggleEditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand AjouterActiviteCommand { get; }
        public ICommand AjouterHebergementCommand { get; }
        public ICommand AjouterNouvelleActiviteCommand { get; }
        public ICommand AjouterNouvelHebergementCommand { get; }
        public ICommand AnnulerAjoutActiviteCommand { get; }
        public ICommand AnnulerAjoutHebergementCommand { get; }
        public ICommand SupprimerActiviteCommand { get; }
        public ICommand SupprimerHebergementCommand { get; }

        public int VoyageId
        {
            get => _voyageId;
            set => SetProperty(ref _voyageId, value);
        }

        public bool IsViewMode
        {
            get => _isViewMode;
            set => SetProperty(ref _isViewMode, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string NomVoyage
        {
            get => _nomVoyage;
            set => SetProperty(ref _nomVoyage, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateOnly DateDebut
        {
            get => _dateDebut;
            set
            {
                if (SetProperty(ref _dateDebut, value) && DateFin < value)
                {
                    DateFin = value;
                }
            }
        }

        public DateOnly DateFin
        {
            get => _dateFin;
            set => SetProperty(ref _dateFin, value);
        }

        public bool EstComplete
        {
            get => _estComplete;
            set => SetProperty(ref _estComplete, value);
        }

        public bool EstArchive
        {
            get => _estArchive;
            set => SetProperty(ref _estArchive, value);
        }

        public int UtilisateurId
        {
            get => _utilisateurId;
            set => SetProperty(ref _utilisateurId, value);
        }

        public bool ShowActiviteForm
        {
            get => _showActiviteForm;
            set => SetProperty(ref _showActiviteForm, value);
        }

        public bool ShowHebergementForm
        {
            get => _showHebergementForm;
            set => SetProperty(ref _showHebergementForm, value);
        }

        public string NouvelleActiviteNom
        {
            get => _nouvelleActiviteNom;
            set => SetProperty(ref _nouvelleActiviteNom, value);
        }

        public string NouvelleActiviteDescription
        {
            get => _nouvelleActiviteDescription;
            set => SetProperty(ref _nouvelleActiviteDescription, value);
        }

        // NOUVEAU : Propriété pour la localisation des activités
        public string NouvelleActiviteLocalisation
        {
            get => _nouvelleActiviteLocalisation;
            set => SetProperty(ref _nouvelleActiviteLocalisation, value);
        }

        public string NouvelHebergementNom
        {
            get => _nouvelHebergementNom;
            set => SetProperty(ref _nouvelHebergementNom, value);
        }

        public string NouvelHebergementType
        {
            get => _nouvelHebergementType;
            set => SetProperty(ref _nouvelHebergementType, value);
        }

        // NOUVEAU : Propriété pour l'adresse des hébergements
        public string NouvelHebergementAdresse
        {
            get => _nouvelHebergementAdresse;
            set => SetProperty(ref _nouvelHebergementAdresse, value);
        }

        public decimal NouvelHebergementCout
        {
            get => _nouvelHebergementCout;
            set => SetProperty(ref _nouvelHebergementCout, value);
        }

        public VoyageDetailsViewModel(IVoyageService voyageService,
                                    IActiviteService activiteService,
                                    IHebergementService hebergementService)
        {
            _voyageService = voyageService;
            _activiteService = activiteService;
            _hebergementService = hebergementService;

            // Commandes principales
            ToggleEditCommand = new Command(ToggleEdit, () => !_isLoading);
            SaveCommand = new Command(async () => await SaveVoyageAsync(), () => !_isLoading && IsEditMode);
            DeleteCommand = new Command(async () => await DeleteVoyageAsync(), () => !_isLoading && IsViewMode);
            CancelEditCommand = new Command(CancelEdit, () => !_isLoading);

            // Commandes pour les activités/hébergements
            AjouterActiviteCommand = new Command(() => ShowActiviteForm = true, () => IsEditMode);
            AjouterHebergementCommand = new Command(() => ShowHebergementForm = true, () => IsEditMode);
            AnnulerAjoutActiviteCommand = new Command(AnnulerAjoutActivite);
            AnnulerAjoutHebergementCommand = new Command(AnnulerAjoutHebergement);
            AjouterNouvelleActiviteCommand = new Command(async () => await AjouterNouvelleActivite(), () => !_isLoading && IsEditMode);
            AjouterNouvelHebergementCommand = new Command(async () => await AjouterNouvelHebergement(), () => !_isLoading && IsEditMode);
            SupprimerActiviteCommand = new Command<Activite>(async (a) => await SupprimerActivite(a), (_) => !_isLoading && IsEditMode);
            SupprimerHebergementCommand = new Command<Hebergement>(async (h) => await SupprimerHebergement(h), (_) => !_isLoading && IsEditMode);
        }

        public async Task LoadVoyageDetails()
        {
            if (_isLoading) return;
            
            try
            {
                _isLoading = true;
                IsBusy = true;

                Debug.WriteLine($"Chargement des détails pour le voyage ID: {VoyageId}");

                var voyageDetails = await _voyageService.GetVoyageDetailsAsync(VoyageId);
                if (voyageDetails == null)
                {
                    Debug.WriteLine("Aucun détail de voyage trouvé");
                    return;
                }

                Debug.WriteLine($"Détails chargés: {voyageDetails.Voyage.NomVoyage}");

                // Mise à jour des propriétés de base sur le thread UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var voyage = voyageDetails.Voyage;
                    
                    NomVoyage = voyage.NomVoyage;
                    Description = voyage.Description;
                    DateDebut = voyage.DateDebut;
                    DateFin = voyage.DateFin;
                    EstComplete = voyage.EstComplete;
                    EstArchive = voyage.EstArchive;
                    UtilisateurId = voyage.UtilisateurId;

                    // Sauvegarder les valeurs originales
                    SaveOriginalValues();

                    // Mise à jour des collections SEULEMENT si elles sont vides
                    if (Activites.Count == 0)
                    {
                        foreach (var activite in voyageDetails.Activites.OrderBy(a => a.Nom))
                        {
                            Activites.Add(activite);
                            Debug.WriteLine($"Activité chargée: {activite.Nom} - Localisation: {activite.Localisation}");
                        }
                    }

                    if (Hebergements.Count == 0)
                    {
                        foreach (var hebergement in voyageDetails.Hebergements.OrderBy(h => h.Nom))
                        {
                            Hebergements.Add(hebergement);
                            Debug.WriteLine($"Hébergement chargé: {hebergement.Nom} - Adresse: {hebergement.Adresse}");
                        }
                    }

                    Debug.WriteLine($"Collections mises à jour: {Activites.Count} activités, {Hebergements.Count} hébergements");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du chargement des détails: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Erreur", $"Impossible de charger les détails du voyage: {ex.Message}", "OK");
                });
            }
            finally
            {
                _isLoading = false;
                IsBusy = false;
                RefreshCommandStates();
            }
        }

        private void SaveOriginalValues()
        {
            _originalNomVoyage = NomVoyage;
            _originalDescription = Description;
            _originalDateDebut = DateDebut;
            _originalDateFin = DateFin;
            _originalActivites = Activites.ToList();
            _originalHebergements = Hebergements.ToList();
        }

        private void RestoreOriginalValues()
        {
            NomVoyage = _originalNomVoyage;
            Description = _originalDescription;
            DateDebut = _originalDateDebut;
            DateFin = _originalDateFin;
            
            Activites.Clear();
            if (_originalActivites != null)
            {
                foreach (var activite in _originalActivites)
                {
                    Activites.Add(activite);
                }
            }
            
            Hebergements.Clear();
            if (_originalHebergements != null)
            {
                foreach (var hebergement in _originalHebergements)
                {
                    Hebergements.Add(hebergement);
                }
            }
        }

        private async Task NotifyVoyageListToRefresh()
        {
            try
            {
                Debug.WriteLine("=== Notification AGRESSIVE de rafraîchissement à la liste des voyages ===");
                
                // Méthode 1: Flags de stockage
                await SecureStorage.SetAsync("needs_voyage_list_refresh", "true");
                Preferences.Set("needs_voyage_list_refresh", true);
                
                // Méthode 2: Timestamp pour forcer le rafraîchissement
                var timestamp = DateTime.Now.Ticks.ToString();
                await SecureStorage.SetAsync("last_voyage_modification", timestamp);
                Preferences.Set("last_voyage_modification", timestamp);
                
                // Méthode 3: Flag spécifique au voyage modifié
                await SecureStorage.SetAsync($"voyage_modified_{VoyageId}", "true");
                Preferences.Set($"voyage_modified_{VoyageId}", true);
                
                Debug.WriteLine($"Flags de rafraîchissement définis - Voyage ID: {VoyageId}, Timestamp: {timestamp}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la notification de rafraîchissement: {ex}");
                // Ne pas faire échouer la sauvegarde pour cette erreur
            }
        }

        private void RefreshCommandStates()
        {
            ((Command)ToggleEditCommand).ChangeCanExecute();
            ((Command)SaveCommand).ChangeCanExecute();
            ((Command)DeleteCommand).ChangeCanExecute();
            ((Command)CancelEditCommand).ChangeCanExecute();
            ((Command)AjouterActiviteCommand).ChangeCanExecute();
            ((Command)AjouterHebergementCommand).ChangeCanExecute();
            ((Command)AjouterNouvelleActiviteCommand).ChangeCanExecute();
            ((Command)AjouterNouvelHebergementCommand).ChangeCanExecute();
            ((Command)SupprimerActiviteCommand).ChangeCanExecute();
            ((Command)SupprimerHebergementCommand).ChangeCanExecute();
        }

        private void ToggleEdit()
        {
            Debug.WriteLine("Basculement en mode édition");
            IsViewMode = false;
            IsEditMode = true;
            RefreshCommandStates();
        }

        private void CancelEdit()
        {
            Debug.WriteLine("Annulation des modifications");
            RestoreOriginalValues();
            
            IsViewMode = true;
            IsEditMode = false;
            ShowActiviteForm = false;
            ShowHebergementForm = false;
            
            RefreshCommandStates();
        }

        private async Task SaveVoyageAsync()
        {
            if (_isLoading) return;
            
            // Validation
            if (string.IsNullOrWhiteSpace(NomVoyage))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom du voyage est requis", "OK");
                return;
            }

            if (DateFin < DateDebut)
            {
                await Shell.Current.DisplayAlert("Erreur", "La date de fin doit être après la date de début", "OK");
                return;
            }

            try
            {
                _isLoading = true;
                IsBusy = true;

                Debug.WriteLine($"Sauvegarde du voyage: {NomVoyage}");

                var voyage = new Voyage
                {
                    VoyageId = VoyageId,
                    NomVoyage = NomVoyage?.Trim(),
                    Description = Description?.Trim(),
                    DateDebut = DateDebut,
                    DateFin = DateFin,
                    EstComplete = EstComplete,
                    EstArchive = EstArchive,
                    UtilisateurId = UtilisateurId,
                    // Créer de nouveaux objets pour éviter les problèmes de tracking
                    Activites = Activites.Select(a => new Activite 
                    { 
                        ActiviteId = a.ActiviteId,
                        Nom = a.Nom,
                        Description = a.Description,
                        Localisation = a.Localisation
                    }).ToList(),
                    Hebergements = Hebergements.Select(h => new Hebergement 
                    { 
                        HebergementId = h.HebergementId,
                        Nom = h.Nom,
                        TypeHebergement = h.TypeHebergement,
                        Cout = h.Cout,
                        DateDebut = h.DateDebut,
                        DateFin = h.DateFin,
                        Adresse = h.Adresse
                    }).ToList()
                };

                await _voyageService.UpdateVoyageAsync(voyage);
                
                Debug.WriteLine("Voyage sauvegardé avec succès");
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsViewMode = true;
                    IsEditMode = false;
                    ShowActiviteForm = false;
                    ShowHebergementForm = false;
                    
                    // Sauvegarder les nouvelles valeurs comme originales
                    SaveOriginalValues();
                });
                
                // NOUVEAU: Notifier qu'une modification a été faite
                await NotifyVoyageListToRefresh();
                
                await Shell.Current.DisplayAlert("Succès", "Voyage mis à jour avec succès", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la sauvegarde: {ex}");
                await Shell.Current.DisplayAlert("Erreur", $"Échec de la mise à jour du voyage: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
                IsBusy = false;
                RefreshCommandStates();
            }
        }

        private async Task DeleteVoyageAsync()
        {
            if (_isLoading) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmer la suppression",
                "Êtes-vous sûr de vouloir supprimer ce voyage?",
                "Oui", "Non");

            if (confirm)
            {
                try
                {
                    _isLoading = true;
                    IsBusy = true;

                    await _voyageService.DeleteVoyageAsync(VoyageId);
                    
                    // NOUVEAU: Notifier qu'une modification a été faite
                    await NotifyVoyageListToRefresh();
                    
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Erreur", $"Échec de la suppression: {ex.Message}", "OK");
                }
                finally
                {
                    _isLoading = false;
                    IsBusy = false;
                    RefreshCommandStates();
                }
            }
        }

        private void AnnulerAjoutActivite()
        {
            NouvelleActiviteNom = string.Empty;
            NouvelleActiviteDescription = string.Empty;
            NouvelleActiviteLocalisation = string.Empty; // NOUVEAU
            ShowActiviteForm = false;
        }

        private void AnnulerAjoutHebergement()
        {
            NouvelHebergementNom = string.Empty;
            NouvelHebergementType = string.Empty;
            NouvelHebergementAdresse = string.Empty; // NOUVEAU
            NouvelHebergementCout = 0;
            ShowHebergementForm = false;
        }

        private async Task AjouterNouvelleActivite()
        {
            if (_isLoading) return;
            
            if (string.IsNullOrWhiteSpace(NouvelleActiviteNom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom de l'activité est requis", "OK");
                return;
            }

            try
            {
                _isLoading = true;
                IsBusy = true;

                var nouvelleActivite = new Activite
                {
                    Nom = NouvelleActiviteNom.Trim(),
                    Description = NouvelleActiviteDescription?.Trim(),
                    Localisation = NouvelleActiviteLocalisation?.Trim() // NOUVEAU
                };

                // Ajout à la base de données
                var activiteAjoutee = await _activiteService.AddActiviteAsync(nouvelleActivite);
                await _voyageService.AddActiviteToVoyageAsync(VoyageId, activiteAjoutee);

                // Ajout à la collection locale sur le thread UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Activites.Add(activiteAjoutee);
                    AnnulerAjoutActivite();
                });

                Debug.WriteLine($"Activité ajoutée: {activiteAjoutee.Nom} - Localisation: {activiteAjoutee.Localisation}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout d'activité: {ex}");
                await Shell.Current.DisplayAlert("Erreur", $"Échec de l'ajout: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
                IsBusy = false;
                RefreshCommandStates();
            }
        }

        private async Task AjouterNouvelHebergement()
        {
            if (_isLoading) return;
            
            if (string.IsNullOrWhiteSpace(NouvelHebergementNom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom de l'hébergement est requis", "OK");
                return;
            }

            try
            {
                _isLoading = true;
                IsBusy = true;

                var nouvelHebergement = new Hebergement
                {
                    Nom = NouvelHebergementNom.Trim(),
                    TypeHebergement = NouvelHebergementType?.Trim(),
                    Adresse = NouvelHebergementAdresse?.Trim(), // NOUVEAU
                    Cout = NouvelHebergementCout
                };

                // Ajout à la base de données
                var hebergementAjoute = await _hebergementService.AddHebergementAsync(nouvelHebergement);
                await _voyageService.AddHebergementToVoyageAsync(VoyageId, hebergementAjoute);

                // Ajout à la collection locale sur le thread UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Hebergements.Add(hebergementAjoute);
                    AnnulerAjoutHebergement();
                });

                Debug.WriteLine($"Hébergement ajouté: {hebergementAjoute.Nom} - Adresse: {hebergementAjoute.Adresse}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout d'hébergement: {ex}");
                await Shell.Current.DisplayAlert("Erreur", $"Échec de l'ajout: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
                IsBusy = false;
                RefreshCommandStates();
            }
        }

        private async Task SupprimerActivite(Activite activite)
        {
            if (_isLoading) return;
            
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmer la suppression",
                $"Supprimer l'activité '{activite.Nom}'?",
                "Oui", "Non");

            if (confirm)
            {
                try
                {
                    _isLoading = true;
                    IsBusy = true;

                    await _voyageService.RemoveActiviteFromVoyageAsync(VoyageId, activite.ActiviteId);
                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Activites.Remove(activite);
                    });
                    Debug.WriteLine($"Activité supprimée: {activite.Nom}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de la suppression d'activité: {ex}");
                    await Shell.Current.DisplayAlert("Erreur", $"Échec de la suppression: {ex.Message}", "OK");
                }
                finally
                {
                    _isLoading = false;
                    IsBusy = false;
                    RefreshCommandStates();
                }
            }
        }

        private async Task SupprimerHebergement(Hebergement hebergement)
        {
            if (_isLoading) return;
            
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmer la suppression",
                $"Supprimer l'hébergement '{hebergement.Nom}'?",
                "Oui", "Non");

            if (confirm)
            {
                try
                {
                    _isLoading = true;
                    IsBusy = true;

                    await _voyageService.RemoveHebergementFromVoyageAsync(VoyageId, hebergement.HebergementId);
                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Hebergements.Remove(hebergement);
                    });

                    Debug.WriteLine($"Hébergement supprimé: {hebergement.Nom}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de la suppression d'hébergement: {ex}");
                    await Shell.Current.DisplayAlert("Erreur", $"Échec de la suppression: {ex.Message}", "OK");
                }
                finally
                {
                    _isLoading = false;
                    IsBusy = false;
                    RefreshCommandStates();
                }
            }
        }
    }
}
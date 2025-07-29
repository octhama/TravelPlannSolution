namespace TravelPlannMauiApp.ViewModels
{
    public class AddVoyageViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private readonly IActiviteService _activiteService;
        private readonly IHebergementService _hebergementService;

        // Propriétés de base pour le voyage
        private string _nomVoyage;
        private string _description;
        private DateTime _dateDebut = DateTime.Today;
        private DateTime _dateFin = DateTime.Today.AddDays(1);

        // Propriétés pour les formulaires d'activités
        private bool _showActiviteForm;
        private string _nouvelleActiviteNom;
        private string _nouvelleActiviteDescription;
        private string _nouvelleActiviteLocalisation;

        // Propriétés pour les formulaires d'hébergements
        private bool _showHebergementForm;
        private string _nouvelHebergementNom;
        private string _nouvelHebergementType;
        private string _nouvelHebergementAdresse;
        private decimal _nouvelHebergementCout;

        // Collections pour les éléments ajoutés
        public ObservableCollection<Activite> NouvellesActivites { get; } = new();
        public ObservableCollection<Hebergement> NouveauxHebergements { get; } = new();

        // Commandes
        public ICommand AddVoyageCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand AjouterActiviteCommand { get; }
        public ICommand AjouterHebergementCommand { get; }
        public ICommand AjouterNouvelleActiviteCommand { get; }
        public ICommand AjouterNouvelHebergementCommand { get; }
        public ICommand AnnulerAjoutActiviteCommand { get; }
        public ICommand AnnulerAjoutHebergementCommand { get; }
        public ICommand SupprimerActiviteCommand { get; }
        public ICommand SupprimerHebergementCommand { get; }

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

        public DateTime DateDebut
        {
            get => _dateDebut;
            set => SetProperty(ref _dateDebut, value);
        }

        public DateTime DateFin
        {
            get => _dateFin;
            set => SetProperty(ref _dateFin, value);
        }

        // Propriétés pour les activités
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

        public string NouvelleActiviteLocalisation
        {
            get => _nouvelleActiviteLocalisation;
            set => SetProperty(ref _nouvelleActiviteLocalisation, value);
        }

        // Propriétés pour les hébergements
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

        public AddVoyageViewModel(IVoyageService voyageService,
                                IActiviteService activiteService,
                                IHebergementService hebergementService)
        {
            _voyageService = voyageService;
            _activiteService = activiteService;
            _hebergementService = hebergementService;

            // Commandes principales
            AddVoyageCommand = new Command(async () => await AddVoyageAsync());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Commandes pour les formulaires
            AjouterActiviteCommand = new Command(() => ShowActiviteForm = true);
            AjouterHebergementCommand = new Command(() => ShowHebergementForm = true);
            AnnulerAjoutActiviteCommand = new Command(() =>
            {
                ResetActiviteForm();
            });
            AnnulerAjoutHebergementCommand = new Command(() =>
            {
                ResetHebergementForm();
            });
            AjouterNouvelleActiviteCommand = new Command(async () => await AjouterNouvelleActivite());
            AjouterNouvelHebergementCommand = new Command(async () => await AjouterNouvelHebergement());

            // Commandes de suppression (suppression locale uniquement)
            SupprimerActiviteCommand = new Command<Activite>(SupprimerActiviteLocale);
            SupprimerHebergementCommand = new Command<Hebergement>(SupprimerHebergementLocal);
        }

        private void ResetActiviteForm()
        {
            NouvelleActiviteNom = string.Empty;
            NouvelleActiviteDescription = string.Empty;
            NouvelleActiviteLocalisation = string.Empty;
            ShowActiviteForm = false;
        }

        private void ResetHebergementForm()
        {
            NouvelHebergementNom = string.Empty;
            NouvelHebergementType = string.Empty;
            NouvelHebergementAdresse = string.Empty;
            NouvelHebergementCout = 0;
            ShowHebergementForm = false;
        }

        private async Task AjouterNouvelleActivite()
        {
            if (string.IsNullOrWhiteSpace(NouvelleActiviteNom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom de l'activité est requis", "OK");
                return;
            }

            try
            {
                var nouvelleActivite = new Activite
                {
                    Nom = NouvelleActiviteNom?.Trim(),
                    Description = NouvelleActiviteDescription?.Trim(),
                    Localisation = NouvelleActiviteLocalisation?.Trim()
                };

                // Créer l'activité d'abord
                var activiteCree = await _activiteService.AddActiviteAsync(nouvelleActivite);

                if (activiteCree != null)
                {
                    NouvellesActivites.Add(activiteCree);
                    ResetActiviteForm();
                    
                    Debug.WriteLine($"Activité ajoutée: {activiteCree.Nom} (ID: {activiteCree.ActiviteId}) - Localisation: {activiteCree.Localisation}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout d'activité: {ex}");
                await Shell.Current.DisplayAlert("Erreur",
                    $"Erreur lors de l'ajout de l'activité: {ex.Message}", "OK");
            }
        }

        private async Task AjouterNouvelHebergement()
        {
            if (string.IsNullOrWhiteSpace(NouvelHebergementNom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom de l'hébergement est requis", "OK");
                return;
            }

            try
            {
                var nouvelHebergement = new Hebergement
                {
                    Nom = NouvelHebergementNom?.Trim(),
                    TypeHebergement = NouvelHebergementType?.Trim(),
                    Adresse = NouvelHebergementAdresse?.Trim(),
                    Cout = NouvelHebergementCout
                };

                // Créer l'hébergement d'abord
                var hebergementCree = await _hebergementService.AddHebergementAsync(nouvelHebergement);

                if (hebergementCree != null)
                {
                    NouveauxHebergements.Add(hebergementCree);
                    ResetHebergementForm();
                    
                    Debug.WriteLine($"Hébergement ajouté: {hebergementCree.Nom} (ID: {hebergementCree.HebergementId}) - Adresse: {hebergementCree.Adresse}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout d'hébergement: {ex}");
                await Shell.Current.DisplayAlert("Erreur",
                    $"Erreur lors de l'ajout de l'hébergement: {ex.Message}", "OK");
            }
        }

        // Suppression locale uniquement (les éléments ne sont pas encore dans la BD pour le voyage)
        private void SupprimerActiviteLocale(Activite activite)
        {
            if (activite != null)
            {
                NouvellesActivites.Remove(activite);
                Debug.WriteLine($"Activité supprimée localement: {activite.Nom}");
            }
        }

        private void SupprimerHebergementLocal(Hebergement hebergement)
        {
            if (hebergement != null)
            {
                NouveauxHebergements.Remove(hebergement);
                Debug.WriteLine($"Hébergement supprimé localement: {hebergement.Nom}");
            }
        }

        private async Task AddVoyageAsync()
        {
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
                IsBusy = true;
                Debug.WriteLine("=== DÉBUT AddVoyageAsync ===");

                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Utilisateur non connecté", "OK");
                    return;
                }

                Debug.WriteLine($"Utilisateur connecté ID: {currentUserId}");

                // Créer le voyage de base
                var voyage = new Voyage
                {
                    NomVoyage = NomVoyage?.Trim(),
                    Description = Description?.Trim(),
                    DateDebut = DateOnly.FromDateTime(DateDebut),
                    DateFin = DateOnly.FromDateTime(DateFin),
                    EstComplete = false,
                    EstArchive = false,
                    UtilisateurId = currentUserId // CRUCIAL : Associer l'utilisateur
                };

                Debug.WriteLine($"Voyage créé avec {NouvellesActivites.Count} activités et {NouveauxHebergements.Count} hébergements à associer");

                // Préparer les collections de relations si nécessaire
                if (NouvellesActivites.Count > 0)
                {
                    // Créer une liste des activités avec uniquement les IDs pour éviter les conflits EF
                    voyage.Activites = NouvellesActivites.Select(a => new Activite { ActiviteId = a.ActiviteId }).ToList();
                    
                    Debug.WriteLine("Activités à associer:");
                    foreach (var activite in NouvellesActivites)
                    {
                        Debug.WriteLine($"  - {activite.Nom} (ID: {activite.ActiviteId}) - Localisation: {activite.Localisation}");
                    }
                }

                if (NouveauxHebergements.Count > 0)
                {
                    // Créer une liste des hébergements avec uniquement les IDs pour éviter les conflits EF
                    voyage.Hebergements = NouveauxHebergements.Select(h => new Hebergement { HebergementId = h.HebergementId }).ToList();
                    
                    Debug.WriteLine("Hébergements à associer:");
                    foreach (var hebergement in NouveauxHebergements)
                    {
                        Debug.WriteLine($"  - {hebergement.Nom} (ID: {hebergement.HebergementId}) - Adresse: {hebergement.Adresse}");
                    }
                }
                
                Debug.WriteLine($"Voyage prêt à être ajouté: {voyage.NomVoyage}, Dates: {voyage.DateDebut} - {voyage.DateFin}, UtilisateurId: {voyage.UtilisateurId}");
                
                await _voyageService.AddVoyageAsync(voyage);
                Debug.WriteLine($"Voyage ajouté avec succès: {voyage.NomVoyage}");
                
                // 1. Définir le flag de rechargement
                Preferences.Set("FORCE_VOYAGE_LIST_RELOAD", true);
                
                // 2. Envoyer un message pour déclencher le rafraîchissement
                MessagingCenter.Send<object>(this, "RefreshVoyageList");
                
                Debug.WriteLine("Flag et message de rafraîchissement envoyés");

                // Réinitialiser les champs
                ResetForm();
                
                // Afficher un message de succès
                await Shell.Current.DisplayAlert("Succès", "Voyage ajouté avec succès!", "OK");
                
                // 3. Retour à la liste des voyages avec navigation forcée
                await Shell.Current.GoToAsync($"..?forceRefresh=true");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== ERREUR AddVoyageAsync ===");
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine($"Type: {ex.GetType().Name}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                }
                
                await HandleError(ex, "Erreur lors de l'ajout du voyage");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("=== FIN AddVoyageAsync ===");
            }
        }

        private void ResetForm()
        {
            NomVoyage = string.Empty;
            Description = string.Empty;
            DateDebut = DateTime.Today;
            DateFin = DateTime.Today.AddDays(1);
            NouvellesActivites.Clear();
            NouveauxHebergements.Clear();
            ShowActiviteForm = false;
            ShowHebergementForm = false;
            ResetActiviteForm();
            ResetHebergementForm();
        }

        // Version asynchrone pour une meilleure gestion des exceptions
        private async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                // Essayer d'abord SecureStorage
                var userIdString = await SecureStorage.GetAsync("current_user_id");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    Debug.WriteLine($"UserId récupéré depuis SecureStorage: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur SecureStorage dans GetCurrentUserIdAsync: {ex.Message}");
            }

            try
            {
                // Fallback vers Preferences
                var userIdString = Preferences.Get("current_user_id", null);
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    Debug.WriteLine($"UserId récupéré depuis Preferences: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur Preferences dans GetCurrentUserIdAsync: {ex.Message}");
            }

            Debug.WriteLine("ERREUR: Impossible de récupérer l'ID utilisateur - aucune session trouvée");
            return 0;
        }

        // Récupération de l'ID de l'utilisateur connecté (version synchrone)
        private int GetCurrentUserId()
        {
            try
            {
                // Essayer d'abord SecureStorage
                var userIdString = SecureStorage.GetAsync("current_user_id").GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    Debug.WriteLine($"UserId récupéré depuis SecureStorage: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur SecureStorage dans GetCurrentUserId: {ex.Message}");
            }

            try
            {
                // Fallback vers Preferences
                var userIdString = Preferences.Get("current_user_id", null);
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    Debug.WriteLine($"UserId récupéré depuis Preferences: {userId}");
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur Preferences dans GetCurrentUserId: {ex.Message}");
            }

            Debug.WriteLine("ERREUR: Impossible de récupérer l'ID utilisateur - aucune session trouvée");
            return 0; // Retourner 0 pour indiquer qu'aucun utilisateur n'est connecté
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing appelé, nettoyage éventuel effectué");
        }
    }
}
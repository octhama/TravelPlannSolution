namespace TravelPlannMauiApp.ViewModels
{
    public class VoyageViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private readonly ISessionService _sessionService;
        private readonly IServiceProvider _serviceProvider;
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
        
        public VoyageViewModel(IVoyageService voyageService, 
                      ISessionService sessionService = null,
                      IServiceProvider serviceProvider = null)
        {
            _voyageService = voyageService;
            _sessionService = sessionService;
            _serviceProvider = serviceProvider;

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
            if (IsLoading) 
            {
                Debug.WriteLine("Chargement déjà en cours - ignoré");
                return;
            }

            await InternalLoadVoyagesAsync();
        }

        // NOUVEAU : Méthode pour forcer le rechargement depuis la base de données
        public async Task ForceReloadFromDatabase()
        {
            Debug.WriteLine("=== FORCE RELOAD FROM DATABASE ===");
            await InternalLoadVoyagesAsync(forceReload: true);
            
            // Forcer la notification de changement
            OnPropertyChanged(nameof(Voyages));
        }

        // NOUVEAU : Méthode interne consolidée pour le chargement
        private async Task InternalLoadVoyagesAsync(bool forceReload = false)
        {
            if (IsLoading && !forceReload) 
            {
                Debug.WriteLine("Chargement déjà en cours - ignoré");
                return;
            }

            IsLoading = true;
            try
            {
                string typeChargement = forceReload ? "RECHARGEMENT FORCÉ" : "RECHARGEMENT NORMAL";
                Debug.WriteLine($"=== DÉBUT {typeChargement} VOYAGES ===");
                
                _currentUserId = await GetCurrentUserIdAsync();
                if (_currentUserId == 0) 
                {
                    Debug.WriteLine("Utilisateur non connecté - arrêt du chargement");
                    return;
                }

                Debug.WriteLine($"Rechargement des voyages pour l'utilisateur ID: {_currentUserId}");
                
                // NOUVEAU: Utiliser un nouveau scope pour chaque opération
                using (var scope = _serviceProvider.CreateScope())
                {
                    var voyageService = scope.ServiceProvider.GetRequiredService<IVoyageService>();
                    
                    // Récupérer les données fraîches de la DB
                    var voyages = await voyageService.GetVoyagesByUtilisateurAsync(_currentUserId);
                    
                    Debug.WriteLine($"Nombre de voyages récupérés de la DB: {voyages?.Count ?? 0}");
                    
                    // Mise à jour FORCÉE sur le thread principal
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Debug.WriteLine("Mise à jour FORCÉE de la collection...");
                        
                        // TOUJOURS vider la collection existante
                        Voyages.Clear();
                        
                        if (voyages != null && voyages.Any())
                        {
                            // Trier les voyages par date de début
                            var voyagesOrdonnes = voyages.OrderBy(x => x.DateDebut).ToList();
                            
                            Debug.WriteLine("Ajout des voyages triés à la collection...");
                            foreach (var v in voyagesOrdonnes)
                            {
                                Debug.WriteLine($"Ajout: {v.NomVoyage} (ID: {v.VoyageId}) - Complete: {v.EstComplete}, Archive: {v.EstArchive}");
                                var voyageItemViewModel = new VoyageItemViewModel(v);
                                Voyages.Add(voyageItemViewModel);
                            }
                        }
                        
                        Debug.WriteLine($"Collection mise à jour - {Voyages.Count} voyages dans la liste");
                        
                        // Forcer PLUSIEURS notifications de changement
                        OnPropertyChanged(nameof(Voyages));
                        OnPropertyChanged(); // Notification générale
                    });
                }
                
                Debug.WriteLine($"=== {typeChargement} TERMINÉ - {Voyages.Count} voyages affichés ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== ERREUR RECHARGEMENT VOYAGES ===");
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Erreur", 
                        $"Erreur lors du rechargement des voyages: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("=== FIN RECHARGEMENT VOYAGES ===");
            }
        }

        // Méthode simple pour définir le flag de rechargement après modification
        private void SetForceReloadFlag()
        {
            try
            {
                Preferences.Set("FORCE_VOYAGE_LIST_RELOAD", true);
                Debug.WriteLine("Flag FORCE_VOYAGE_LIST_RELOAD défini");
                
                // NOUVEAU : Envoyer aussi un message pour déclencher le rafraîchissement immédiatement
                MessagingCenter.Send<object>(this, "RefreshVoyageList");
                Debug.WriteLine("Message RefreshVoyageList envoyé");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la définition du flag: {ex}");
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

                Debug.WriteLine($"Toggle Complete pour voyage {voyage.NomVoyage} - Ancien statut: {voyage.EstComplete}");

                // Récupérer le voyage complet avec ses relations
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

                await _voyageService.UpdateVoyageAsync(voyageToUpdate);
                
                Debug.WriteLine($"Voyage {voyage.NomVoyage} mis à jour - Nouveau statut Complete: {voyageToUpdate.EstComplete}");
                
                // Mise à jour immédiate de l'item dans la collection
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    voyageViewModel.UpdateFromVoyage(voyageToUpdate);
                    OnPropertyChanged(nameof(Voyages));
                });
                
                // Définir le flag pour forcer le rechargement
                SetForceReloadFlag();
                
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

                Debug.WriteLine($"Toggle Archive pour voyage {voyage.NomVoyage} - Ancien statut: {voyage.EstArchive}");

                // Récupérer le voyage complet avec ses relations
                var voyageDetails = await _voyageService.GetVoyageDetailsAsync(voyage.VoyageId);
                if (voyageDetails?.Voyage == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Voyage introuvable", "OK");
                    return;
                }

                var voyageToUpdate = voyageDetails.Voyage;
                voyageToUpdate.EstArchive = !voyageToUpdate.EstArchive;
                
                await _voyageService.UpdateVoyageAsync(voyageToUpdate);
                
                Debug.WriteLine($"Voyage {voyage.NomVoyage} mis à jour - Nouveau statut Archive: {voyageToUpdate.EstArchive}");
                
                // Mise à jour immédiate de l'item dans la collection
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    voyageViewModel.UpdateFromVoyage(voyageToUpdate);
                    OnPropertyChanged(nameof(Voyages));
                });
                
                // Définir le flag pour forcer le rechargement
                SetForceReloadFlag();
                
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
                Debug.WriteLine($"Navigation vers VoyageDetailsPage avec {activitesDto.Count} activités et {hebergementsDto.Count} hébergements");

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
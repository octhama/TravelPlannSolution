using TravelPlannMauiApp.ViewModels;
using System.Diagnostics;

namespace TravelPlannMauiApp.Pages
{
    public partial class VoyageListPage : ContentPage
    {
        private readonly VoyageViewModel _viewModel;
        private bool _isFirstLoad = true;
        private bool _shouldRefreshOnAppearing = false;

        public VoyageListPage(VoyageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            
            // S'abonner aux événements de navigation Shell
            Shell.Current.Navigated += OnShellNavigated;
        }

        private async void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"=== Navigation détectée ===");
                Debug.WriteLine($"Source: {e.Source}");
                Debug.WriteLine($"Current: {e.Current?.Location}");
                Debug.WriteLine($"Previous: {e.Previous?.Location}");

                // Vérifier si on revient vers cette page depuis une page de détails ou d'ajout
                if (e.Current?.Location?.ToString().Contains("VoyageListPage") == true)
                {
                    var previousLocation = e.Previous?.Location?.ToString();
                    
                    if (previousLocation?.Contains("VoyageDetailsPage") == true || 
                        previousLocation?.Contains("AddVoyagePage") == true)
                    {
                        Debug.WriteLine("Retour détecté depuis une page de modification - marquage pour rafraîchissement");
                        _shouldRefreshOnAppearing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur OnShellNavigated: {ex}");
            }
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try 
            {
                Debug.WriteLine("=== OnAppearing - VoyageListPage ===");
                Debug.WriteLine($"_isFirstLoad: {_isFirstLoad}");
                Debug.WriteLine($"_shouldRefreshOnAppearing: {_shouldRefreshOnAppearing}");
                Debug.WriteLine($"Voyages count: {_viewModel?.Voyages?.Count ?? 0}");
                
                // Vérifier le flag de rafraîchissement stocké
                bool needsRefresh = await CheckAndClearRefreshFlag();
                
                if (BindingContext is VoyageViewModel viewModel)
                {
                    // Cas 1: Premier chargement ET aucune donnée
                    if (_isFirstLoad && viewModel.Voyages.Count == 0)
                    {
                        Debug.WriteLine("Premier chargement - chargement des données");
                        await viewModel.LoadVoyagesAsync();
                    }
                    // Cas 2: Flag de rafraîchissement détecté
                    else if (needsRefresh)
                    {
                        Debug.WriteLine("Flag de rafraîchissement détecté - rechargement des données");
                        await viewModel.LoadVoyagesAsync();
                    }
                    // Cas 3: Retour depuis une page de modification
                    else if (_shouldRefreshOnAppearing)
                    {
                        Debug.WriteLine("Rafraîchissement après modification détectée");
                        await viewModel.LoadVoyagesAsync();
                        _shouldRefreshOnAppearing = false;
                    }
                    // Cas 4: Rafraîchissement périodique (si les données sont anciennes)
                    else if (!_isFirstLoad && ShouldRefreshData())
                    {
                        Debug.WriteLine("Rafraîchissement périodique des données");
                        await viewModel.LoadVoyagesAsync();
                    }
                    else
                    {
                        Debug.WriteLine("Aucun rafraîchissement nécessaire");
                        // Forcer une mise à jour de l'affichage sans recharger les données
                        ForceUIRefresh();
                    }
                    
                    _isFirstLoad = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur OnAppearing: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du rafraîchissement de la liste", "OK");
            }
        }

        // Vérifier et effacer le flag de rafraîchissement
        private async Task<bool> CheckAndClearRefreshFlag()
        {
            try
            {
                // Vérifier dans SecureStorage d'abord
                var flagSecure = await SecureStorage.GetAsync("needs_voyage_list_refresh");
                if (!string.IsNullOrEmpty(flagSecure) && flagSecure == "true")
                {
                    await SecureStorage.SetAsync("needs_voyage_list_refresh", "false");
                    Debug.WriteLine("Flag de rafraîchissement trouvé dans SecureStorage");
                    return true;
                }
                
                // Vérifier dans Preferences comme alternative
                bool flagPrefs = Preferences.Get("needs_voyage_list_refresh", false);
                if (flagPrefs)
                {
                    Preferences.Set("needs_voyage_list_refresh", false);
                    Debug.WriteLine("Flag de rafraîchissement trouvé dans Preferences");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la vérification du flag de rafraîchissement: {ex}");
                return false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("=== OnDisappearing - VoyageListPage ===");
        }

        // Vérifier si les données doivent être rafraîchies (par exemple, si elles sont anciennes)
        private bool ShouldRefreshData()
        {
            // Vous pouvez implémenter une logique de cache ici
            // Par exemple, rafraîchir si les données ont plus de 5 minutes
            return false; // Pour l'instant, pas de rafraîchissement automatique
        }

        // Forcer une mise à jour de l'affichage sans recharger les données
        private void ForceUIRefresh()
        {
            try
            {
                if (_viewModel != null)
                {
                    Debug.WriteLine("Forçage de la mise à jour de l'UI");
                    
                    // Déclencher une notification de changement sur la collection
                    _viewModel.OnPropertyChanged(nameof(_viewModel.Voyages));
                    
                    // Forcer la mise à jour de chaque élément
                    foreach (var voyage in _viewModel.Voyages)
                    {
                        voyage.OnPropertyChanged(string.Empty); // Mise à jour de toutes les propriétés
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ForceUIRefresh: {ex}");
            }
        }

        // Méthode publique pour forcer le rafraîchissement (peut être appelée depuis d'autres pages)
        public async Task RefreshDataAsync()
        {
            try
            {
                Debug.WriteLine("=== RefreshDataAsync appelé manuellement ===");
                if (BindingContext is VoyageViewModel viewModel)
                {
                    await viewModel.LoadVoyagesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur RefreshDataAsync: {ex}");
            }
        }

        // Méthode pour marquer qu'un rafraîchissement est nécessaire
        public void MarkForRefresh()
        {
            Debug.WriteLine("=== Marquage pour rafraîchissement ===");
            _shouldRefreshOnAppearing = true;
        }
        
        // Nettoyage lors de la destruction de la page
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            
            if (Handler == null)
            {
                // Désabonnement des événements pour éviter les fuites mémoire
                Shell.Current.Navigated -= OnShellNavigated;
            }
        }

        // Gestionnaire pour le bouton d'ajout (si vous l'utilisez encore)
        private async void OnAddVoyageClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Navigation vers AddVoyagePage");
                await Shell.Current.GoToAsync(nameof(AddVoyagePage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex}");
                await DisplayAlert("Erreur", "Impossible d'ouvrir la page d'ajout", "OK");
            }
        }
    }
}
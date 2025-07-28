using TravelPlannMauiApp.ViewModels;
using System.Diagnostics;

namespace TravelPlannMauiApp.Pages
{
    public partial class VoyageListPage : ContentPage
    {
        private readonly VoyageViewModel _viewModel;
        private string _lastUpdateTimestamp = "";

        public VoyageListPage(VoyageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try 
            {
                Debug.WriteLine("=== OnAppearing - VoyageListPage ===");
                
                if (BindingContext is VoyageViewModel viewModel)
                {
                    // NOUVEAU : Vérifier si un rafraîchissement est nécessaire
                    await CheckAndRefreshIfNeeded();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur OnAppearing: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du rechargement de la liste", "OK");
            }
        }

        // NOUVEAU : Méthode pour vérifier et rafraîchir si nécessaire
        private async Task CheckAndRefreshIfNeeded()
        {
            try
            {
                Debug.WriteLine("=== VÉRIFICATION DU BESOIN DE RAFRAÎCHISSEMENT ===");
                
                // Vérifier le flag de rafraîchissement
                bool needsRefresh = Preferences.Get("voyage_list_needs_refresh", false);
                string currentTimestamp = Preferences.Get("last_voyage_update_timestamp", "");
                
                Debug.WriteLine($"Needs refresh flag: {needsRefresh}");
                Debug.WriteLine($"Current timestamp: {currentTimestamp}");
                Debug.WriteLine($"Last known timestamp: {_lastUpdateTimestamp}");
                
                // Conditions pour rafraîchir :
                // 1. Le flag needs_refresh est true
                // 2. Le timestamp a changé (nouvelle modification)
                // 3. Première fois qu'on charge la page (_lastUpdateTimestamp vide)
                
                bool shouldRefresh = needsRefresh || 
                                   (!string.IsNullOrEmpty(currentTimestamp) && currentTimestamp != _lastUpdateTimestamp) ||
                                   string.IsNullOrEmpty(_lastUpdateTimestamp);
                
                if (shouldRefresh)
                {
                    Debug.WriteLine("🔄 RAFRAÎCHISSEMENT NÉCESSAIRE - Rechargement des données...");
                    
                    // Recharger les données depuis la base
                    await _viewModel.LoadVoyagesAsync();
                    
                    // Mettre à jour le timestamp local
                    _lastUpdateTimestamp = currentTimestamp;
                    
                    // Réinitialiser le flag
                    Preferences.Set("voyage_list_needs_refresh", false);
                    
                    Debug.WriteLine("✅ Rafraîchissement terminé");
                }
                else
                {
                    Debug.WriteLine("ℹ️ Aucun rafraîchissement nécessaire - données à jour");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la vérification du rafraîchissement: {ex}");
                // En cas d'erreur, faire un rechargement complet par sécurité
                await _viewModel.LoadVoyagesAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("=== OnDisappearing - VoyageListPage ===");
        }

        // NOUVEAU : Méthode publique pour forcer le rechargement immédiat
        public async Task ForceRefreshAsync()
        {
            try
            {
                Debug.WriteLine("=== ForceRefreshAsync - Rechargement forcé ===");
                if (_viewModel != null)
                {
                    await _viewModel.LoadVoyagesAsync();
                    
                    // Mettre à jour le timestamp pour éviter les doubles rechargements
                    _lastUpdateTimestamp = Preferences.Get("last_voyage_update_timestamp", "");
                    Preferences.Set("voyage_list_needs_refresh", false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ForceRefreshAsync: {ex}");
            }
        }

        // NOUVEAU : Méthode pour rafraîchir manuellement (Pull-to-refresh)
        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Rafraîchissement manuel demandé ===");
                
                if (_viewModel != null)
                {
                    await _viewModel.LoadVoyagesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du rafraîchissement manuel: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du rafraîchissement", "OK");
            }
        }
    }
}
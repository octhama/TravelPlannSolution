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
                Debug.WriteLine("=== VÉRIFICATION SIMPLE DU BESOIN DE RAFRAÎCHISSEMENT ===");
                
                // Vérifier le flag SIMPLE
                bool forceReload = Preferences.Get("FORCE_VOYAGE_LIST_RELOAD", false);
                
                Debug.WriteLine($"Force reload flag: {forceReload}");
                
                if (forceReload)
                {
                    Debug.WriteLine("🔄 RECHARGEMENT FORCÉ DÉTECTÉ - Rechargement des données...");
                    
                    // Recharger les données depuis la base
                    await _viewModel.LoadVoyagesAsync();
                    
                    // Réinitialiser le flag
                    Preferences.Set("FORCE_VOYAGE_LIST_RELOAD", false);
                    
                    Debug.WriteLine("✅ Rafraîchissement FORCÉ terminé");
                }
                else
                {
                    Debug.WriteLine("ℹ️ Aucun rafraîchissement forcé nécessaire");
                    
                    // Faire un chargement normal si la liste est vide
                    if (_viewModel.Voyages.Count == 0)
                    {
                        Debug.WriteLine("Liste vide - chargement initial");
                        await _viewModel.LoadVoyagesAsync();
                    }
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
                    
                    // Réinitialiser le flag
                    Preferences.Set("FORCE_VOYAGE_LIST_RELOAD", false);
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
using TravelPlannMauiApp.ViewModels;
using System.Diagnostics;

namespace TravelPlannMauiApp.Pages
{
    public partial class VoyageListPage : ContentPage
    {
        private readonly VoyageViewModel _viewModel;

        public VoyageListPage(VoyageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            
            // S'abonner aux messages de rafraîchissement
            MessagingCenter.Subscribe<object>(this, "RefreshVoyageList", async (sender) =>
            {
                Debug.WriteLine("=== MESSAGE REFRESH REÇU ===");
                await RefreshVoyageListFromMessage();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try 
            {
                Debug.WriteLine("=== OnAppearing - VoyageListPage ===");
                
                // Vérifier si le paramètre forceRefresh est présent
                if (Shell.Current?.CurrentState?.Location?.OriginalString?.Contains("forceRefresh=true") ?? false)
                {
                    Debug.WriteLine("Forcer le rechargement via paramètre URL");
                    await _viewModel.ForceReloadFromDatabase();
                }
                else
                {
                    // Logique normale de vérification du flag
                    await CheckAndForceRefreshIfNeeded();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur OnAppearing: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du rechargement de la liste", "OK");
            }
        }

        // NOUVEAU : Méthode qui FORCE systématiquement le rechargement si flag présent
        private async Task CheckAndForceRefreshIfNeeded()
        {
            try
            {
                Debug.WriteLine("=== VÉRIFICATION SYSTÉMATIQUE DU FLAG ===");
                
                // Vérifier le flag
                bool forceReload = Preferences.Get("FORCE_VOYAGE_LIST_RELOAD", false);
                
                Debug.WriteLine($"Force reload flag: {forceReload}");
                
                if (forceReload)
                {
                    Debug.WriteLine("🔄 FLAG DÉTECTÉ - RECHARGEMENT FORCÉ IMMÉDIAT");
                    
                    // Réinitialiser le flag IMMÉDIATEMENT pour éviter les boucles
                    Preferences.Set("FORCE_VOYAGE_LIST_RELOAD", false);
                    
                    // Forcer le rechargement depuis la base de données
                    await _viewModel.ForceReloadFromDatabase();
                    
                    Debug.WriteLine("✅ Rechargement forcé terminé");
                }
                else
                {
                    Debug.WriteLine("ℹ️ Pas de flag - chargement normal si nécessaire");
                    
                    // Chargement normal si la liste est vide
                    if (_viewModel.Voyages.Count == 0)
                    {
                        Debug.WriteLine("Liste vide - chargement initial");
                        await _viewModel.LoadVoyagesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la vérification: {ex}");
                // En cas d'erreur, faire un rechargement de sécurité
                await _viewModel.LoadVoyagesAsync();
            }
        }

        // NOUVEAU : Méthode appelée par les messages
        private async Task RefreshVoyageListFromMessage()
        {
            try
            {
                Debug.WriteLine("=== RAFRAÎCHISSEMENT VIA MESSAGE ===");
                await _viewModel.ForceReloadFromDatabase();
                Debug.WriteLine("✅ Rafraîchissement via message terminé");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur rafraîchissement via message: {ex}");
            }
        }

        // Méthode publique pour forcer le rechargement
        public async Task ForceRefreshAsync()
        {
            try
            {
                Debug.WriteLine("=== ForceRefreshAsync - Rechargement public ===");
                if (_viewModel != null)
                {
                    await _viewModel.ForceReloadFromDatabase();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ForceRefreshAsync: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("=== OnDisappearing - VoyageListPage ===");
        }

        // Nettoyage des abonnements
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        // NOUVEAU : Méthode pour nettoyer les abonnements
        ~VoyageListPage()
        {
            MessagingCenter.Unsubscribe<object>(this, "RefreshVoyageList");
        }
    }
}
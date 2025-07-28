using TravelPlannMauiApp.ViewModels;
using System.Diagnostics;

namespace TravelPlannMauiApp.Pages
{
    public partial class VoyageListPage : ContentPage
    {
        private readonly VoyageViewModel _viewModel;
        private bool _isFirstLoad = true;

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
                    // Toujours recharger les données quand la page apparaît
                    // sauf si c'est le premier chargement et que les données sont déjà présentes
                    if (!_isFirstLoad || viewModel.Voyages.Count == 0)
                    {
                        Debug.WriteLine("Rechargement des voyages...");
                        await viewModel.LoadVoyagesAsync();
                    }
                    else
                    {
                        Debug.WriteLine("Premier chargement - données déjà présentes");
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("=== OnDisappearing - VoyageListPage ===");
        }

        // Méthode pour forcer le rafraîchissement (peut être appelée depuis d'autres pages)
        public async Task RefreshDataAsync()
        {
            try
            {
                Debug.WriteLine("=== RefreshDataAsync appelé ===");
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
        
        // Ajoutez cette méthode pour gérer le clic sur le bouton Ajouter
        private async void OnAddVoyageClicked(object sender, EventArgs e)
        {
            try
            {
                // Utilisez la navigation Shell pour une meilleure cohérence
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
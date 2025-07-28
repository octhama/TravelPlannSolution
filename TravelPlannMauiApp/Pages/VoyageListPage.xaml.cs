// VoyageListPage.xaml.cs - Version SIMPLE
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
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try 
            {
                Debug.WriteLine("=== OnAppearing - VoyageListPage - RECHARGEMENT AUTOMATIQUE ===");
                
                if (BindingContext is VoyageViewModel viewModel)
                {
                    // SIMPLE : Toujours recharger les données depuis la DB
                    Debug.WriteLine("Rechargement des voyages depuis la base de données...");
                    await viewModel.LoadVoyagesAsync();
                    Debug.WriteLine($"Rechargement terminé - {viewModel.Voyages.Count} voyages affichés");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur OnAppearing: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du rechargement de la liste", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("=== OnDisappearing - VoyageListPage ===");
        }

        // Méthode publique pour forcer le rechargement (optionnel)
        public async Task RefreshDataAsync()
        {
            try
            {
                Debug.WriteLine("=== RefreshDataAsync - Rechargement manuel ===");
                if (_viewModel != null)
                {
                    await _viewModel.LoadVoyagesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur RefreshDataAsync: {ex}");
            }
        }
    }
}
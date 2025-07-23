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
                if (BindingContext is VoyageViewModel viewModel)
                {
                    await viewModel.LoadVoyagesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refresh error: {ex}");
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
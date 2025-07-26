using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TravelPlannMauiApp.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public SettingsPage()
        {
            InitializeComponent();
            
            // Créer le ViewModel manuellement si l'injection échoue
            try
            {
                var serviceProvider = Handler?.MauiContext?.Services ?? 
                                    (Application.Current as App)?.Handler?.MauiContext?.Services;
                
                if (serviceProvider != null)
                {
                    _viewModel = serviceProvider.GetService<SettingsViewModel>();
                }
                
                if (_viewModel == null)
                {
                    // Créer une instance temporaire en cas d'échec de l'injection
                    System.Diagnostics.Debug.WriteLine("ATTENTION: SettingsViewModel non injecté, création manuelle");
                    DisplayAlert("Erreur", "Impossible de charger les paramètres", "OK");
                    return;
                }
                
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur initialisation SettingsPage: {ex}");
                DisplayAlert("Erreur", "Erreur lors de l'initialisation de la page", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                if (_viewModel?.LoadSettingsCommand?.CanExecute(null) == true)
                {
                    _viewModel.LoadSettingsCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur OnAppearing SettingsPage: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du chargement des paramètres", "OK");
            }
        }
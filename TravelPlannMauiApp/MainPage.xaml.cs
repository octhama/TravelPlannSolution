using Microsoft.Maui.Controls;
using TravelPlannMauiApp.Pages;
using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using BU.Services;

namespace TravelPlannMauiApp
{
    public partial class MainPage : ContentPage
    {
        private int _currentTab = 1; // 0: Map, 1: Home, 2: Trips
        private readonly IServiceProvider _serviceProvider;

        public MainPage()
        {
            InitializeComponent();
            
            // Obtenir le service provider de manière plus sûre
            _serviceProvider = Handler?.MauiContext?.Services ?? 
                              (Application.Current as App)?.Handler?.MauiContext?.Services;
            
            // Créer et assigner le ViewModel
            if (_serviceProvider != null)
            {
                var viewModel = _serviceProvider.GetService<MainPageViewModel>();
                if (viewModel == null)
                {
                    var voyageService = _serviceProvider.GetService<IVoyageService>();
                    var sessionService = _serviceProvider.GetService<ISessionService>();
                    if (voyageService != null && sessionService != null)
                    {
                        viewModel = new MainPageViewModel(voyageService, sessionService);
                    }
                }
                BindingContext = viewModel;
            }
            
            // Fallback avec un ViewModel minimal si les services ne sont pas disponibles
            if (BindingContext == null)
            {
                BindingContext = new MainPageViewModel(null, null);
            }
            
            UpdateTabSelection();
            UpdateIndicatorPosition();
            
            // S'abonner à l'événement de navigation
            NavigationPage.SetHasNavigationBar(this, false);
            this.Appearing += OnPageAppearing;
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Réinitialiser à l'onglet Accueil quand la page réapparaît
            _currentTab = 1;
            UpdateTabSelection();
            UpdateIndicatorPosition();
            
            // Recharger les informations utilisateur
            if (BindingContext is MainPageViewModel viewModel)
            {
                _ = viewModel.LoadUserInfoAsync();
            }
        }

        private async void OnNextTripTapped(object sender, EventArgs e)
        {
            try
            {
                if (_serviceProvider == null)
                {
                    await DisplayAlert("Erreur", "Services non disponibles", "OK");
                    return;
                }

                var voyageViewModel = _serviceProvider.GetService<VoyageViewModel>();
                if (voyageViewModel == null)
                {
                    await DisplayAlert("Erreur", "ViewModel Voyage non disponible", "OK");
                    return;
                }

                var voyageListPage = new VoyageListPage(voyageViewModel);
                await Navigation.PushAsync(voyageListPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers voyages: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdateIndicatorPosition();
        }

        protected async void OnSettingsTapped(object sender, EventArgs e)
        {
            try
            {
                var settingsPage = new SettingsPage();
                await Navigation.PushAsync(settingsPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers paramètres: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        private void UpdateIndicatorPosition()
        {
            if (Width <= 0 || Height <= 0) return;

            var tabWidth = Width / 3;
            IndicatorBar.TranslationX = tabWidth * _currentTab + (tabWidth / 2) - 30;
        }

        private async void OnTabTapped(object sender, EventArgs e)
        {
            try
            {
                var grid = (Grid)sender;
                var tabIndex = Grid.GetColumn(grid);

                if (_currentTab == tabIndex)
                    return;

                _currentTab = tabIndex;
                UpdateTabSelection();
                UpdateIndicatorPosition();

                if (_serviceProvider == null)
                {
                    await DisplayAlert("Erreur", "Services non disponibles", "OK");
                    return;
                }

                switch (tabIndex)
                {
                    case 0: // Map
                        var mapViewModel = _serviceProvider.GetService<MapViewModel>();
                        if (mapViewModel != null)
                        {
                            var mapPage = new MapPage(mapViewModel);
                            await Navigation.PushAsync(mapPage);
                        }
                        else
                        {
                            await DisplayAlert("Erreur", "ViewModel Carte non disponible", "OK");
                        }
                        break;
                        
                    case 2: // Trips
                        var voyageViewModel = _serviceProvider.GetService<VoyageViewModel>();
                        if (voyageViewModel != null)
                        {
                            var voyageListPage = new VoyageListPage(voyageViewModel);
                            await Navigation.PushAsync(voyageListPage);
                        }
                        else
                        {
                            await DisplayAlert("Erreur", "ViewModel Voyage non disponible", "OK");
                        }
                        break;
                        
                    case 1: // Home
                    default:
                        // Ne rien faire pour l'onglet Accueil
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du tap sur onglet: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
                
                // Remettre l'onglet précédent en cas d'erreur
                _currentTab = 1;
                UpdateTabSelection();
                UpdateIndicatorPosition();
            }
        }

        private void UpdateTabSelection()
        {
            // Réinitialiser toutes les couleurs
            MapLabel.TextColor = Color.FromArgb("#666666");
            HomeLabel.TextColor = Color.FromArgb("#666666");
            TripsLabel.TextColor = Color.FromArgb("#666666");

            // Mettre en surbrillance l'onglet actif
            switch (_currentTab)
            {
                case 0:
                    MapLabel.TextColor = Color.FromArgb("#6200EE");
                    break;
                case 1:
                    HomeLabel.TextColor = Color.FromArgb("#6200EE");
                    break;
                case 2:
                    TripsLabel.TextColor = Color.FromArgb("#6200EE");
                    break;
            }
        }
    }
}
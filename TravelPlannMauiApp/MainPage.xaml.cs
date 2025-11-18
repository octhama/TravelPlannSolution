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
        private IServiceProvider _serviceProvider;

        public MainPage()
        {
            InitializeComponent();

            // Pour obtenir le service provider de manière plus sûre
            _serviceProvider = GetServiceProvider();

            // Pour créer et assigner le ViewModel
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

            // Pour s'abonner à l'événement de navigation
            NavigationPage.SetHasNavigationBar(this, false);
            this.Appearing += OnPageAppearing;
        }

        private IServiceProvider GetServiceProvider()
        {
            return Handler?.MauiContext?.Services ??
                   (Application.Current as App)?.Handler?.MauiContext?.Services ??
                   (Application.Current as App)?.Windows?.FirstOrDefault()?.Handler?.MauiContext?.Services;
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Réinitialisation de l'onglet actif à l'onglet Accueil quand la page réapparaît
            _currentTab = 1;
            UpdateTabSelection();
            UpdateIndicatorPosition();

            // Rechargement des  informations utilisateur
            if (BindingContext is MainPageViewModel viewModel)
            {
                _ = viewModel.LoadUserInfoAsync();
            }
        }

        private async void OnNextTripTapped(object sender, EventArgs e)
        {
            try
            {
                // Essayer de récupérer le service provider s'il n'est pas disponible
                if (_serviceProvider == null)
                {
                    _serviceProvider = GetServiceProvider();
                }

                if (_serviceProvider == null)
                {
                    await DisplayAlert("Erreur", "Services non disponibles - impossible d'ajouter un voyage.", "OK");
                    return;
                }

                // Créer le ViewModel avec les services requis
                var voyageService = _serviceProvider.GetService<IVoyageService>();
                var activiteService = _serviceProvider.GetService<IActiviteService>();
                var hebergementService = _serviceProvider.GetService<IHebergementService>();

                var addVoyageViewModel = new AddVoyageViewModel(voyageService, activiteService, hebergementService);
                var addVoyagePage = new AddVoyagePage(addVoyageViewModel);
                await Navigation.PushAsync(addVoyagePage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers nouveau voyage: {ex}");
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

        private async void OnReservationsTapped(object sender, EventArgs e)
        {
            try
            {
                var reservationPage = new ReservationPage();
                await Navigation.PushAsync(reservationPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers réservations: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        private async void OnGroupsTapped(object sender, EventArgs e)
        {
            try
            {
                var groupPage = new GroupManagementPage();
                await Navigation.PushAsync(groupPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers groupes: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        private async void OnLeaderboardTapped(object sender, EventArgs e)
        {
            try
            {
                var leaderboardPage = new LeaderboardPage();
                await Navigation.PushAsync(leaderboardPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers classement: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        private async void OnRewardsTapped(object sender, EventArgs e)
        {
            try
            {
                var rewardsPage = new RewardsPage();
                await Navigation.PushAsync(rewardsPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers récompenses: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            try
            {
                if (_serviceProvider == null)
                {
                    await DisplayAlert("Erreur", "Services non disponibles", "OK");
                    return;
                }

                var profileViewModel = _serviceProvider.GetService<ProfileViewModel>();
                if (profileViewModel != null)
                {
                    // Pour l'instant, on redirige vers les paramètres car il n'y a pas de page profil dédiée
                    var settingsPage = new SettingsPage();
                    await Navigation.PushAsync(settingsPage);
                }
                else
                {
                    await DisplayAlert("Erreur", "ViewModel Profil non disponible", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers profil: {ex}");
                await DisplayAlert("Erreur", $"Erreur de navigation: {ex.Message}", "OK");
            }
        }

        private async void OnNewTripTapped(object sender, EventArgs e)
        {
            try
            {
                // Essayer de récupérer le service provider s'il n'est pas disponible
                if (_serviceProvider == null)
                {
                    _serviceProvider = GetServiceProvider();
                }

                if (_serviceProvider == null)
                {
                    await DisplayAlert("Erreur", "Services non disponibles - impossible d'ajouter un voyage.", "OK");
                    return;
                }

                // Créer le ViewModel avec les services requis
                var voyageService = _serviceProvider.GetService<IVoyageService>();
                var activiteService = _serviceProvider.GetService<IActiviteService>();
                var hebergementService = _serviceProvider.GetService<IHebergementService>();

                var addVoyageViewModel = new AddVoyageViewModel(voyageService, activiteService, hebergementService);
                var addVoyagePage = new AddVoyagePage(addVoyageViewModel);
                await Navigation.PushAsync(addVoyagePage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation vers nouveau voyage: {ex}");
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
                    _serviceProvider = GetServiceProvider();
                    if (_serviceProvider == null)
                    {
                        await DisplayAlert("Erreur", "Services non disponibles", "OK");
                        return;
                    }
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
                        // Créer le VoyageViewModel avec tous les services requis
                        var voyageService = _serviceProvider.GetService<IVoyageService>();
                        var sessionService = _serviceProvider.GetService<ISessionService>();
                        
                        var voyageViewModel = new VoyageViewModel(voyageService, sessionService, _serviceProvider);
                        var voyageListPage = new VoyageListPage(voyageViewModel);
                        await Navigation.PushAsync(voyageListPage);
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

                // Pour remettre l'onglet précédent en cas d'erreur
                _currentTab = 1;
                UpdateTabSelection();
                UpdateIndicatorPosition();
            }
        }

        private void UpdateTabSelection()
        {
            // Réinitialisation de toutes les couleurs
            MapLabel.TextColor = Color.FromArgb("#666666");
            HomeLabel.TextColor = Color.FromArgb("#666666");
            TripsLabel.TextColor = Color.FromArgb("#666666");

            // Pour mettre en surbrillance l'onglet actif
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
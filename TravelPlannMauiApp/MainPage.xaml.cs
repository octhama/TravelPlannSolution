using Microsoft.Maui.Controls;
using TravelPlannMauiApp.Pages;
using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp
{
    public partial class MainPage : ContentPage
    {
        private int _currentTab = 1; // 0: Map, 1: Home, 2: Trips
        private MainPageViewModel _viewModel; // ViewModel for MainPage

        public MainPage()
        {
            InitializeComponent();

            _viewModel = Handler.MauiContext.Services.GetService<MainPageViewModel>();
        BindingContext = _viewModel;
            UpdateTabSelection();
            UpdateIndicatorPosition();
            
            // S'abonner à l'événement de navigation
            NavigationPage.SetHasNavigationBar(this, false);
            this.Appearing += OnPageAppearing;
        }

        protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ChargerDonnees();
    }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Réinitialiser à l'onglet Accueil quand la page réapparaît
            _currentTab = 1;
            UpdateTabSelection();
            UpdateIndicatorPosition();
        }

        private async void OnNextTripTapped(object sender, EventArgs e)
        {
            var voyageListPage = new VoyageListPage(
                Handler.MauiContext.Services.GetService<VoyageViewModel>());
            await Navigation.PushAsync(voyageListPage);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdateIndicatorPosition();
        }

        protected async void OnSettingsTapped(object sender, EventArgs e)
        {
            var settingsPage = new SettingsPage();
            await Navigation.PushAsync(settingsPage);
        }

        private void UpdateIndicatorPosition()
        {
            if (Width <= 0 || Height <= 0) return;

            var tabWidth = Width / 3;
            IndicatorBar.TranslationX = tabWidth * _currentTab + (tabWidth / 2) - 30;
        }

        private async void OnTabTapped(object sender, EventArgs e)
        {
            var grid = (Grid)sender;
            var tabIndex = Grid.GetColumn(grid);

            if (_currentTab == tabIndex)
                return;

            _currentTab = tabIndex;
            UpdateTabSelection();
            UpdateIndicatorPosition();

            switch (tabIndex)
            {
                case 0:
                    await Navigation.PushAsync(new MapPage());
                    break;
                case 2:
                    var voyageListPage = new VoyageListPage(
                        Handler.MauiContext.Services.GetService<VoyageViewModel>());
                    await Navigation.PushAsync(voyageListPage);
                    break;
                case 1:
                default:
                    // Ne rien faire pour l'onglet Accueil
                    break;
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
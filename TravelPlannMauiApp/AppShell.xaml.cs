using TravelPlannMauiApp.Pages;
using TravelPlannMauiApp.ViewModels;
using BU.Services;

namespace TravelPlannMauiApp
{
    public partial class AppShell : Shell

    {
        private readonly IAuthService _authService;
        public AppShell(IAuthService authService)
        {
            _authService = authService;
            InitializeComponent();

            VerifierAuthentification();

            Routing.RegisterRoute(nameof(VoyageDetailsPage), typeof(VoyageDetailsPage));
            Routing.RegisterRoute(nameof(VoyageListPage), typeof(VoyageListPage));
            Routing.RegisterRoute(nameof(AddVoyagePage), typeof(AddVoyagePage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
        }

        private async void VerifierAuthentification()
        {
            if (!_authService.EstConnecte)
            {
                await GoToAsync("//ConnexionPage");
            }
        }

        
    }
}
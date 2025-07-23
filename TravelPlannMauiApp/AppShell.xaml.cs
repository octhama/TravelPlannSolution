using TravelPlannMauiApp.Pages;

namespace TravelPlannMauiApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(VoyageDetailsPage), typeof(VoyageDetailsPage));
            Routing.RegisterRoute(nameof(VoyageListPage), typeof(VoyageListPage));
            Routing.RegisterRoute(nameof(AddVoyagePage), typeof(AddVoyagePage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
        }
    }
}
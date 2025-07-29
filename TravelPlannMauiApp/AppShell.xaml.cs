namespace TravelPlannMauiApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            try
            {
                // Pages principales
                Routing.RegisterRoute(nameof(VoyageDetailsPage), typeof(VoyageDetailsPage));
                Routing.RegisterRoute(nameof(VoyageListPage), typeof(VoyageListPage));
                Routing.RegisterRoute(nameof(AddVoyagePage), typeof(AddVoyagePage));
                Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
                Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
                
                // Pages d'authentification
                Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
                Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));

                System.Diagnostics.Debug.WriteLine("Routes enregistrées avec succès");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'enregistrement des routes: {ex}");
            }
        }
    }
}
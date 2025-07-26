using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages
{
    public partial class MapPage : ContentPage
    {
        private readonly MapViewModel _viewModel;

        public MapPage(MapViewModel viewModel)
        {
            InitializeComponent();
            
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                // Configurer la WebView dans le ViewModel
                _viewModel.SetWebView(MapWebView);
                
                // Charger le contenu HTML de la carte
                LoadMapContent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation de la carte: {ex}");
                DisplayAlert("Erreur", $"Impossible d'initialiser la carte: {ex.Message}", "OK");
            }
        }

        private void LoadMapContent()
        {
            // Ici, vous devriez charger votre contenu HTML pour la carte
            // Par exemple, une carte Leaflet ou Google Maps
            var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Carte</title>
    <style>
        body { margin: 0; padding: 0; font-family: Arial, sans-serif; }
        #map { height: 100vh; width: 100%; }
        .placeholder { 
            display: flex; 
            justify-content: center; 
            align-items: center; 
            height: 100vh; 
            background: #f0f0f0;
            color: #666;
            font-size: 18px;
        }
    </style>
</head>
<body>
    <div class='placeholder'>
        <div>
            <h2>🗺️ Carte</h2>
            <p>La carte sera affichée ici</p>
            <p>Intégrez ici votre solution de cartographie<br>(Leaflet, Google Maps, etc.)</p>
        </div>
    </div>
    
    <script>
        // Fonctions appelées depuis l'application
        function searchLocationFromApp(query) {
            console.log('Recherche:', query);
            // Implémentez la recherche ici
        }
        
        function toggleViewModeFromApp() {
            console.log('Changement de vue');
            return '🌍'; // Retourner l'icône appropriée
        }
        
        function goToMyLocationFromApp() {
            console.log('Aller à ma position');
            // Implémentez la géolocalisation ici
        }
        
        function zoomInFromApp() {
            console.log('Zoom in');
            // Implémentez le zoom ici
        }
        
        function zoomOutFromApp() {
            console.log('Zoom out');
            // Implémentez le zoom ici
        }
    </script>
</body>
</html>";

            MapWebView.Source = new HtmlWebViewSource { Html = htmlContent };
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Nettoyer si nécessaire
        }
    }
}
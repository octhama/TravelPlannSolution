using TravelPlannMauiApp.ViewModels;


namespace TravelPlannMauiApp.Pages;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;
    private bool _isMapLoaded = false;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        LoadMap();
    }

    private void LoadMap()
    {
        // HTML pour une carte 3D légère utilisant Leaflet avec plugin 3D
        var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>TravelPlann Map</title>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <style>
        body, html { 
            margin: 0; 
            padding: 0; 
            height: 100%; 
            font-family: Arial, sans-serif;
        }
        #map { 
            height: 100vh; 
            width: 100%;
        }
        .custom-marker {
            background-color: #6200EE;
            border: 3px solid white;
            border-radius: 50%;
            box-shadow: 0 2px 10px rgba(0,0,0,0.3);
        }
        .location-popup {
            font-family: Arial, sans-serif;
            text-align: center;
        }
        .location-popup h3 {
            margin: 0 0 10px 0;
            color: #6200EE;
        }
        .location-popup p {
            margin: 5px 0;
            color: #666;
        }
    </style>
</head>
<body>
    <div id='map'></div>
    
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <script>
        let map;
        let currentMarker = null;
        let is3DMode = false;
        let currentLayer;
        
        // Initialisation de la carte
        function initMap() {
            map = L.map('map', {
                center: [48.8566, 2.3522], // Paris par défaut
                zoom: 13,
                zoomControl: false,
                attributionControl: false
            });

            // Couche de base
            currentLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors'
            }).addTo(map);

            // Événement de clic sur la carte
            map.on('click', function(e) {
                addMarker(e.latlng);
                getLocationInfo(e.latlng);
            });

            // Notification que la carte est chargée
            window.mapLoaded = true;
        }

        // Ajouter un marqueur
        function addMarker(latlng, title = '') {
            if (currentMarker) {
                map.removeLayer(currentMarker);
            }

            const icon = L.divIcon({
                className: 'custom-marker',
                iconSize: [20, 20],
                iconAnchor: [10, 10]
            });

            currentMarker = L.marker(latlng, { icon: icon }).addTo(map);

            if (title) {
                currentMarker.bindPopup(`
                    <div class='location-popup'>
                        <h3>${title}</h3>
                        <p>Lat: ${latlng.lat.toFixed(6)}</p>
                        <p>Lng: ${latlng.lng.toFixed(6)}</p>
                    </div>
                `).openPopup();
            }
        }

        // Obtenir les informations de localisation
        function getLocationInfo(latlng) {
            // Géocodage inversé simple
            fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${latlng.lat}&lon=${latlng.lng}`)
                .then(response => response.json())
                .then(data => {
                    const locationName = data.display_name || 'Lieu inconnu';
                    const address = data.address || {};
                    
                    addMarker(latlng, locationName);
                    
                    // Communiquer avec l'application MAUI (si disponible)
                    if (window.webkit?.messageHandlers?.locationSelected) {
                        window.webkit.messageHandlers.locationSelected.postMessage({
                            name: locationName,
                            address: locationName,
                            lat: latlng.lat,
                            lng: latlng.lng
                        });
                    }
                })
                .catch(error => {
                    console.log('Erreur de géocodage:', error);
                    addMarker(latlng, 'Lieu sélectionné');
                });
        }

        // Rechercher une localisation
        function searchLocation(query) {
            if (!query) return;
            
            fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}`)
                .then(response => response.json())
                .then(data => {
                    if (data && data.length > 0) {
                        const result = data[0];
                        const latlng = L.latLng(parseFloat(result.lat), parseFloat(result.lon));
                        map.setView(latlng, 15);
                        addMarker(latlng, result.display_name);
                    }
                })
                .catch(error => {
                    console.log('Erreur de recherche:', error);
                });
        }

        // Changer de mode de vue
        function toggleViewMode() {
            if (is3DMode) {
                // Retour en 2D
                map.removeLayer(currentLayer);
                currentLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '© OpenStreetMap contributors'
                }).addTo(map);
                is3DMode = false;
            } else {
                // Mode satellite/3D
                map.removeLayer(currentLayer);
                currentLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                    attribution: '© Esri'
                }).addTo(map);
                is3DMode = true;
            }
        }

        // Aller à ma position
        function goToMyLocation() {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function(position) {
                    const latlng = L.latLng(position.coords.latitude, position.coords.longitude);
                    map.setView(latlng, 15);
                    addMarker(latlng, 'Ma position');
                });
            }
        }

        // Fonctions de zoom
        function zoomIn() {
            map.zoomIn();
        }

        function zoomOut() {
            map.zoomOut();
        }

        // Initialiser la carte au chargement
        document.addEventListener('DOMContentLoaded', initMap);
    </script>
</body>
</html>";

        MapWebView.Source = new HtmlWebViewSource { Html = htmlContent };
        MapWebView.Navigated += OnMapNavigated;
    }

    private async void OnMapNavigated(object sender, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success && !_isMapLoaded)
        {
            _isMapLoaded = true;
            // Attendre que la carte soit complètement chargée
            await Task.Delay(1000);

            // Injecter les fonctions JavaScript pour la communication
            await MapWebView.EvaluateJavaScriptAsync(@"
                window.searchLocationFromApp = function(query) {
                    searchLocation(query);
                };
                
                window.toggleViewModeFromApp = function() {
                    toggleViewMode();
                    return is3DMode ? '🌍' : '🗺️';
                };
                
                window.goToMyLocationFromApp = function() {
                    goToMyLocation();
                };
                
                window.zoomInFromApp = function() {
                    zoomIn();
                };
                
                window.zoomOutFromApp = function() {
                    zoomOut();
                };
            ");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SetWebView(MapWebView);
    }
}
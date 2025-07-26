using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using TravelPlannMauiApp.ViewModels;
using Microsoft.Maui.Controls;

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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                // Configurer le contrôle Map dans le ViewModel
                _viewModel.SetMapControl(MapControl);
                
                // Initialiser la carte avec la position par défaut (Paris)
                await InitializeMapAsync();
                
                // Configurer les événements de la carte
                SetupMapEvents();
                
                // Rafraîchir les données utilisateur
                await _viewModel.RefreshDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation de la carte: {ex}");
                await DisplayAlert("Erreur", $"Impossible d'initialiser la carte: {ex.Message}", "OK");
            }
        }

        private async Task InitializeMapAsync()
        {
            try
            {
                // Position par défaut : Paris, France
                var parisLocation = new Location(48.8566, 2.3522);
                var mapSpan = MapSpan.FromCenterAndRadius(parisLocation, Distance.FromKilometers(10));
                
                MapControl.MoveToRegion(mapSpan);
                
                // Demander la permission de géolocalisation
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status == PermissionStatus.Granted)
                {
                    MapControl.IsShowingUser = true;
                    
                    // Essayer d'obtenir la position actuelle
                    try
                    {
                        var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Medium,
                            Timeout = TimeSpan.FromSeconds(5)
                        });

                        if (location != null)
                        {
                            var userMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(5));
                            MapControl.MoveToRegion(userMapSpan);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Impossible d'obtenir la position actuelle: {ex.Message}");
                        // Rester sur Paris si la géolocalisation échoue
                    }
                }
                else
                {
                    MapControl.IsShowingUser = false;
                    System.Diagnostics.Debug.WriteLine("Permission de géolocalisation refusée");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation de la carte: {ex.Message}");
            }
        }

        private void SetupMapEvents()
        {
            // Événement lorsqu'un pin est sélectionné
            MapControl.MapClicked += OnMapClicked;
            
            // Vous pouvez ajouter d'autres événements ici si nécessaire
            // MapControl.PropertyChanged += OnMapPropertyChanged;
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            try
            {
                // Fermer le panel d'informations si l'utilisateur clique ailleurs
                _viewModel.CloseLocationInfoCommand?.Execute(null);
                
                // Optionnel : Ajouter un pin temporaire à l'emplacement cliqué
                var clickedLocation = e.Location;
                System.Diagnostics.Debug.WriteLine($"Carte cliquée à: {clickedLocation.Latitude}, {clickedLocation.Longitude}");
                
                // Vous pouvez implémenter une logique pour afficher des informations
                // sur le lieu cliqué ou effectuer une recherche inverse de géocodage
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du clic sur la carte: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            try
            {
                // Nettoyer les événements pour éviter les fuites mémoire
                if (MapControl != null)
                {
                    MapControl.MapClicked -= OnMapClicked;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage de la carte: {ex.Message}");
            }
        }

        // Méthode utilitaire pour centrer la carte sur un lieu spécifique
        public void CenterMapOn(Location location, double radiusKm = 5)
        {
            try
            {
                if (location != null)
                {
                    var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(radiusKm));
                    MapControl.MoveToRegion(mapSpan);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du centrage de la carte: {ex.Message}");
            }
        }

        // Méthode pour ajouter un pin personnalisé
        public void AddCustomPin(Location location, string label, string address)
        {
            try
            {
                var pin = new Pin
                {
                    Location = location,
                    Label = label,
                    Address = address,
                    Type = PinType.Place
                };

                MapControl.Pins.Add(pin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout du pin: {ex.Message}");
            }
        }

        // Méthode pour nettoyer tous les pins
        public void ClearAllPins()
        {
            try
            {
                MapControl.Pins.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage des pins: {ex.Message}");
            }
        }
    }
}
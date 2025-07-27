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
                
                System.Diagnostics.Debug.WriteLine("MapPage initialisée avec succès");
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
                            Timeout = TimeSpan.FromSeconds(10)
                        });

                        if (location != null)
                        {
                            // Informer le ViewModel de la position utilisateur
                            _viewModel.UpdateUserLocation(location);
                            
                            var userMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(5));
                            MapControl.MoveToRegion(userMapSpan);
                            
                            System.Diagnostics.Debug.WriteLine($"Position utilisateur obtenue: {location.Latitude}, {location.Longitude}");
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
                throw;
            }
        }

        private void SetupMapEvents()
        {
            try
            {
                // Événement lorsque la carte est cliquée
                MapControl.MapClicked += OnMapClicked;
                
                // Événement pour les changements de propriétés de la carte
                MapControl.PropertyChanged += OnMapPropertyChanged;
                
                System.Diagnostics.Debug.WriteLine("Événements de la carte configurés");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la configuration des événements: {ex.Message}");
            }
        }

        private async void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            try
            {
                var clickedLocation = e.Location;
                System.Diagnostics.Debug.WriteLine($"Carte cliquée à: {clickedLocation.Latitude}, {clickedLocation.Longitude}");
                
                // Fermer le panel d'informations si ouvert
                _viewModel.CloseLocationInfoCommand?.Execute(null);
                
                // Optionnel : Effectuer un géocodage inverse pour obtenir l'adresse
                var address = await _viewModel.GetAddressFromLocationAsync(clickedLocation);
                
                // Créer un pin temporaire à l'emplacement cliqué
                var tempPin = new Pin
                {
                    Location = clickedLocation,
                    Label = "Lieu sélectionné",
                    Address = address,
                    Type = PinType.Generic
                };
                
                // Supprimer les anciens pins temporaires
                var existingTempPins = MapControl.Pins
                    .Where(p => p.Label == "Lieu sélectionné")
                    .ToList();
                
                foreach (var pin in existingTempPins)
                {
                    MapControl.Pins.Remove(pin);
                }
                
                // Ajouter le nouveau pin temporaire
                MapControl.Pins.Add(tempPin);
                
                // Afficher les détails du lieu
                _viewModel.ShowLocationDetails(tempPin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du clic sur la carte: {ex.Message}");
            }
        }

        private void OnMapPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(MapControl.VisibleRegion))
                {
                    var region = MapControl.VisibleRegion;
                    if (region != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Région visible changée - Centre: {region.Center.Latitude}, {region.Center.Longitude}, Rayon: {region.Radius.Meters}m");
                        
                        // Informer le ViewModel du changement de région
                        _viewModel.OnMapRegionChanged(region);
                    }
                }
                else if (e.PropertyName == nameof(MapControl.Pins))
                {
                    System.Diagnostics.Debug.WriteLine($"Pins mis à jour - Total: {MapControl.Pins.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du traitement des changements de propriétés: {ex.Message}");
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
                    MapControl.PropertyChanged -= OnMapPropertyChanged;
                }
                
                System.Diagnostics.Debug.WriteLine("MapPage nettoyée");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage de la carte: {ex.Message}");
            }
        }

        // Méthodes utilitaires publiques

        /// <summary>
        /// Centre la carte sur un lieu spécifique
        /// </summary>
        public void CenterMapOn(Location location, double radiusKm = 5)
        {
            try
            {
                if (location != null && MapControl != null)
                {
                    var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(radiusKm));
                    MapControl.MoveToRegion(mapSpan);
                    System.Diagnostics.Debug.WriteLine($"Carte centrée sur: {location.Latitude}, {location.Longitude}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du centrage de la carte: {ex.Message}");
            }
        }

        /// <summary>
        /// Ajoute un pin personnalisé à la carte
        /// </summary>
        public void AddCustomPin(Location location, string label, string address, PinType type = PinType.Place)
        {
            try
            {
                if (location != null && !string.IsNullOrEmpty(label))
                {
                    var pin = new Pin
                    {
                        Location = location,
                        Label = label,
                        Address = address ?? "",
                        Type = type
                    };

                    MapControl?.Pins.Add(pin);
                    System.Diagnostics.Debug.WriteLine($"Pin ajouté: {label} à {location.Latitude}, {location.Longitude}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout du pin: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime tous les pins d'un type spécifique
        /// </summary>
        public void RemovePinsByType(PinType type)
        {
            try
            {
                if (MapControl?.Pins != null)
                {
                    var pinsToRemove = MapControl.Pins.Where(p => p.Type == type).ToList();
                    
                    foreach (var pin in pinsToRemove)
                    {
                        MapControl.Pins.Remove(pin);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"{pinsToRemove.Count} pins de type {type} supprimés");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression des pins: {ex.Message}");
            }
        }

        /// <summary>
        /// Nettoie tous les pins sauf ceux spécifiés
        /// </summary>
        public void ClearPinsExcept(params string[] labelsToKeep)
        {
            try
            {
                if (MapControl?.Pins != null)
                {
                    var pinsToRemove = MapControl.Pins
                        .Where(p => !labelsToKeep.Contains(p.Label))
                        .ToList();
                    
                    foreach (var pin in pinsToRemove)
                    {
                        MapControl.Pins.Remove(pin);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"{pinsToRemove.Count} pins supprimés, {labelsToKeep.Length} conservés");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage sélectif des pins: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime tous les pins de la carte
        /// </summary>
        public void ClearAllPins()
        {
            try
            {
                if (MapControl?.Pins != null)
                {
                    var pinCount = MapControl.Pins.Count;
                    MapControl.Pins.Clear();
                    System.Diagnostics.Debug.WriteLine($"{pinCount} pins supprimés");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage des pins: {ex.Message}");
            }
        }

        /// <summary>
        /// Anime la carte vers une nouvelle région
        /// </summary>
        public async Task AnimateToRegionAsync(MapSpan region, int durationMs = 1000)
        {
            try
            {
                if (region != null && MapControl != null)
                {
                    // MAUI Maps ne supporte pas l'animation native, on utilise MoveToRegion
                    MapControl.MoveToRegion(region);
                    
                    // Simuler une animation avec un délai
                    await Task.Delay(Math.Min(durationMs, 2000));
                    
                    System.Diagnostics.Debug.WriteLine($"Carte animée vers: {region.Center.Latitude}, {region.Center.Longitude}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'animation: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtient la région actuellement visible sur la carte
        /// </summary>
        public MapSpan GetCurrentRegion()
        {
            try
            {
                return MapControl?.VisibleRegion;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'obtention de la région: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Vérifie si une location est visible dans la région actuelle
        /// </summary>
        public bool IsLocationVisible(Location location)
        {
            try
            {
                var region = GetCurrentRegion();
                if (region == null || location == null)
                    return false;

                var distance = Location.CalculateDistance(region.Center, location, DistanceUnits.Kilometers) * 1000; // Convert kilometers to meters
                return distance <= region.Radius.Meters;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la vérification de visibilité: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ajuste la carte pour afficher tous les pins visibles
        /// </summary>
        public void FitAllPins(double paddingKm = 2)
        {
            try
            {
                if (MapControl?.Pins == null || !MapControl.Pins.Any())
                    return;

                var pins = MapControl.Pins.ToList();
                
                var minLat = pins.Min(p => p.Location.Latitude);
                var maxLat = pins.Max(p => p.Location.Latitude);
                var minLon = pins.Min(p => p.Location.Longitude);
                var maxLon = pins.Max(p => p.Location.Longitude);

                var centerLat = (minLat + maxLat) / 2;
                var centerLon = (minLon + maxLon) / 2;
                var center = new Location(centerLat, centerLon);

                // Calculer le rayon nécessaire pour inclure tous les pins
                var maxDistance = pins.Max(p => Location.CalculateDistance(center, p.Location, DistanceUnits.Kilometers));
                var radius = Math.Max(maxDistance + paddingKm, 1); // Minimum 1km

                var region = MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(radius));
                MapControl.MoveToRegion(region);

                System.Diagnostics.Debug.WriteLine($"Carte ajustée pour {pins.Count} pins - Rayon: {radius:F1}km");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajustement aux pins: {ex.Message}");
            }
        }
    }
}
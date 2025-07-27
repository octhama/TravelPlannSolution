using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class MapViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IVoyageService _voyageService;
    private readonly IActiviteService _activiteService;
    private readonly IHebergementService _hebergementService;
    private readonly ISessionService _sessionService;

    // Contrôle de carte
    private Microsoft.Maui.Controls.Maps.Map _mapControl;
    
    // Propriétés de l'interface utilisateur
    private string _searchQuery = "";
    private bool _isLoading = false;
    private bool _showLocationInfo = false;
    private bool _showFilters = false;
    private bool _showMessage = false;
    private string _messageText = "";
    
    // Propriétés de localisation sélectionnée
    private string _selectedLocationName = "";
    private string _selectedLocationAddress = "";
    private string _selectedLocationDistance = "";
    
    // Propriétés d'icônes
    private string _viewModeIcon = "🗺️";
    private string _mapStyleIcon = "🌙";
    
    // Propriétés de filtres
    private bool _showAccommodations = true;
    private bool _showActivities = true;
    private bool _showRestaurants = false;
    private bool _showTransport = false;
    
    // Position actuelle de l'utilisateur
    private Location _userLocation;
    private MapSpan _currentRegion;
    
    // Collections pour les points d'intérêt
    private List<Pin> _accommodationPins = new();
    private List<Pin> _activityPins = new();
    private List<Pin> _restaurantPins = new();
    private List<Pin> _transportPins = new();

    // Données des voyages
    private List<Voyage> _userVoyages = new();
    
    // État de la carte
    private bool _isMapInitialized = false;
    private CancellationTokenSource _messagesCancellationTokenSource;

    public event PropertyChangedEventHandler PropertyChanged;

    #region Properties

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool ShowLocationInfo
    {
        get => _showLocationInfo;
        set
        {
            _showLocationInfo = value;
            OnPropertyChanged();
        }
    }

    public bool ShowFilters
    {
        get => _showFilters;
        set
        {
            _showFilters = value;
            OnPropertyChanged();
        }
    }

    public bool ShowMessage
    {
        get => _showMessage;
        set
        {
            _showMessage = value;
            OnPropertyChanged();
        }
    }

    public string MessageText
    {
        get => _messageText;
        set
        {
            _messageText = value;
            OnPropertyChanged();
        }
    }

    public string SelectedLocationName
    {
        get => _selectedLocationName;
        set
        {
            _selectedLocationName = value;
            OnPropertyChanged();
        }
    }

    public string SelectedLocationAddress
    {
        get => _selectedLocationAddress;
        set
        {
            _selectedLocationAddress = value;
            OnPropertyChanged();
        }
    }

    public string SelectedLocationDistance
    {
        get => _selectedLocationDistance;
        set
        {
            _selectedLocationDistance = value;
            OnPropertyChanged();
        }
    }

    public string ViewModeIcon
    {
        get => _viewModeIcon;
        set
        {
            _viewModeIcon = value;
            OnPropertyChanged();
        }
    }

    public string MapStyleIcon
    {
        get => _mapStyleIcon;
        set
        {
            _mapStyleIcon = value;
            OnPropertyChanged();
        }
    }

    public bool ShowAccommodations
    {
        get => _showAccommodations;
        set
        {
            _showAccommodations = value;
            OnPropertyChanged();
            _ = Task.Run(UpdateMapPins); // Exécuter de manière asynchrone
        }
    }

    public bool ShowActivities
    {
        get => _showActivities;
        set
        {
            _showActivities = value;
            OnPropertyChanged();
            _ = Task.Run(UpdateMapPins); // Exécuter de manière asynchrone
        }
    }

    public bool ShowRestaurants
    {
        get => _showRestaurants;
        set
        {
            _showRestaurants = value;
            OnPropertyChanged();
            _ = Task.Run(UpdateMapPins); // Exécuter de manière asynchrone
        }
    }

    public bool ShowTransport
    {
        get => _showTransport;
        set
        {
            _showTransport = value;
            OnPropertyChanged();
            _ = Task.Run(UpdateMapPins); // Exécuter de manière asynchrone
        }
    }

    #endregion

    #region Commands

    public ICommand SearchCommand { get; }
    public ICommand ToggleViewModeCommand { get; }
    public ICommand ToggleMapStyleCommand { get; }
    public ICommand GoToMyLocationCommand { get; }
    public ICommand ToggleFiltersCommand { get; }
    public ICommand CloseLocationInfoCommand { get; }
    public ICommand ShowAccommodationsCommand { get; }
    public ICommand ShowActivitiesCommand { get; }
    public ICommand ShowRestaurantsCommand { get; }
    public ICommand ShowDirectionsCommand { get; }
    
    // Commandes de zoom uniquement (navigation directionnelle supprimée)
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }

    #endregion

    public MapViewModel(IVoyageService voyageService, 
                      IActiviteService activiteService, 
                      IHebergementService hebergementService,
                      ISessionService sessionService)
    {
        _voyageService = voyageService ?? throw new ArgumentNullException(nameof(voyageService));
        _activiteService = activiteService ?? throw new ArgumentNullException(nameof(activiteService));
        _hebergementService = hebergementService ?? throw new ArgumentNullException(nameof(hebergementService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

        // Initialiser les commandes
        SearchCommand = new Command(async () => await ExecuteSearchCommand());
        ToggleViewModeCommand = new Command(async () => await ExecuteToggleViewModeCommand());
        ToggleMapStyleCommand = new Command(async () => await ExecuteToggleMapStyleCommand());
        GoToMyLocationCommand = new Command(async () => await ExecuteGoToMyLocationCommand());
        ToggleFiltersCommand = new Command(() => ExecuteToggleFiltersCommand());
        CloseLocationInfoCommand = new Command(() => ExecuteCloseLocationInfoCommand());
        ShowAccommodationsCommand = new Command(() => ExecuteShowAccommodationsCommand());
        ShowActivitiesCommand = new Command(() => ExecuteShowActivitiesCommand());
        ShowRestaurantsCommand = new Command(() => ExecuteShowRestaurantsCommand());
        ShowDirectionsCommand = new Command(() => ExecuteShowDirectionsCommand());
        
        // Commandes de zoom uniquement
        ZoomInCommand = new Command(() => ExecuteZoomInCommand());
        ZoomOutCommand = new Command(() => ExecuteZoomOutCommand());
        
        // Initialiser le cancellation token pour les messages
        _messagesCancellationTokenSource = new CancellationTokenSource();
        
        // Charger les données utilisateur de manière asynchrone
        _ = LoadUserDataAsync();
    }

    #region Map Control Management

    public void SetMapControl(Microsoft.Maui.Controls.Maps.Map mapControl)
    {
        _mapControl = mapControl;
        _isMapInitialized = mapControl != null;
        
        if (_mapControl != null)
        {
            // Configurer les propriétés de base
            _mapControl.IsZoomEnabled = true;
            _mapControl.IsScrollEnabled = true;
            _mapControl.InputTransparent = false;
            
            // Configurer les événements
            SetupMapEvents();
            
            // Mettre à jour les pins avec les données déjà chargées
            _ = Task.Run(UpdateMapPins);
            
            System.Diagnostics.Debug.WriteLine("Contrôle de carte configuré avec succès");
        }
    }
    
    private void SetupMapEvents()
    {
        if (_mapControl != null)
        {
            _mapControl.PropertyChanged += OnMapPropertyChanged;
            System.Diagnostics.Debug.WriteLine("Événements de carte configurés");
        }
    }
    
    private void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(Microsoft.Maui.Controls.Maps.Map.VisibleRegion))
            {
                _currentRegion = _mapControl?.VisibleRegion;
                if (_currentRegion != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Région carte mise à jour: {_currentRegion.Center.Latitude:F4}, {_currentRegion.Center.Longitude:F4}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur dans OnMapPropertyChanged: {ex.Message}");
        }
    }

    public void OnMapRegionChanged(MapSpan newRegion)
    {
        _currentRegion = newRegion;
        System.Diagnostics.Debug.WriteLine($"Région changée via MapPage: {newRegion?.Center.Latitude:F4}, {newRegion?.Center.Longitude:F4}");
    }

    public void UpdateUserLocation(Location location)
    {
        _userLocation = location;
        System.Diagnostics.Debug.WriteLine($"Position utilisateur mise à jour: {location?.Latitude:F4}, {location?.Longitude:F4}");
    }

    #endregion

    #region Command Implementations

    private async Task ExecuteSearchCommand()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || !_isMapInitialized)
        {
            ShowTemporaryMessage("Saisissez un lieu à rechercher");
            return;
        }

        IsLoading = true;
        try
        {
            var searchResult = await GeocodeLocationAsync(SearchQuery);
            
            if (searchResult != null)
            {
                // Centrer la carte sur le résultat
                var mapSpan = MapSpan.FromCenterAndRadius(searchResult, Distance.FromKilometers(2));
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _mapControl.MoveToRegion(mapSpan);
                });
                
                // Supprimer les anciens pins de recherche
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var existingSearchPins = _mapControl.Pins.Where(p => p.Type == PinType.SearchResult).ToList();
                    foreach (var pin in existingSearchPins)
                    {
                        _mapControl.Pins.Remove(pin);
                    }
                });
                
                // Ajouter un nouveau pin de recherche
                var searchPin = new Pin
                {
                    Location = searchResult,
                    Label = SearchQuery,
                    Address = await GetAddressFromLocationAsync(searchResult),
                    Type = PinType.SearchResult
                };
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _mapControl.Pins.Add(searchPin);
                });
                
                ShowLocationDetails(searchPin);
                ShowTemporaryMessage($"Lieu trouvé: {SearchQuery}");
                
                // Effacer la recherche
                SearchQuery = "";
            }
            else
            {
                ShowTemporaryMessage("Aucun résultat trouvé pour cette recherche");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de recherche: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de la recherche");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteToggleViewModeCommand()
    {
        if (!_isMapInitialized) return;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                switch (_mapControl.MapType)
                {
                    case MapType.Street:
                        _mapControl.MapType = MapType.Satellite;
                        ViewModeIcon = "🛰️";
                        ShowTemporaryMessage("Mode satellite");
                        break;
                    case MapType.Satellite:
                        _mapControl.MapType = MapType.Hybrid;
                        ViewModeIcon = "🌍";
                        ShowTemporaryMessage("Mode hybride");
                        break;
                    case MapType.Hybrid:
                        _mapControl.MapType = MapType.Street;
                        ViewModeIcon = "🗺️";
                        ShowTemporaryMessage("Mode plan");
                        break;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de mode vue: {ex.Message}");
        }
    }

    private async Task ExecuteToggleMapStyleCommand()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Cette fonctionnalité dépendrait du thème de l'application
                // Pour l'instant, on change juste l'icône
                if (MapStyleIcon == "🌙")
                {
                    MapStyleIcon = "☀️";
                    ShowTemporaryMessage("Thème sombre activé");
                }
                else
                {
                    MapStyleIcon = "🌙";
                    ShowTemporaryMessage("Thème clair activé");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de style: {ex.Message}");
        }
    }

    private async Task ExecuteGoToMyLocationCommand()
    {
        if (!_isMapInitialized) return;

        IsLoading = true;
        try
        {
            // Vérifier d'abord les permissions
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                ShowTemporaryMessage("Permission de localisation requise");
                return;
            }

            // Obtenir la position actuelle
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                UpdateUserLocation(location);
                
                // Centrer la carte sur la position utilisateur
                var userMapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(2));
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _mapControl.MoveToRegion(userMapSpan);
                });
                
                ShowTemporaryMessage("Position actuelle localisée");
                System.Diagnostics.Debug.WriteLine($"Centré sur position utilisateur: {location.Latitude:F4}, {location.Longitude:F4}");
            }
            else
            {
                ShowTemporaryMessage("Impossible d'obtenir votre position");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur géolocalisation: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de la localisation");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteToggleFiltersCommand()
    {
        ShowFilters = !ShowFilters;
        if (ShowFilters)
        {
            ShowTemporaryMessage("Filtres d'affichage");
        }
    }

    private void ExecuteCloseLocationInfoCommand()
    {
        ShowLocationInfo = false;
        SelectedLocationName = "";
        SelectedLocationAddress = "";
        SelectedLocationDistance = "";
    }

    private void ExecuteShowAccommodationsCommand()
    {
        ShowTemporaryMessage("Recherche d'hébergements...");
        // TODO: Implémenter la recherche d'hébergements autour du lieu sélectionné
    }

    private void ExecuteShowActivitiesCommand()
    {
        ShowTemporaryMessage("Recherche d'activités...");
        // TODO: Implémenter la recherche d'activités autour du lieu sélectionné
    }

    private void ExecuteShowRestaurantsCommand()
    {
        ShowTemporaryMessage("Recherche de restaurants...");
        // TODO: Implémenter la recherche de restaurants autour du lieu sélectionné
    }

    private void ExecuteShowDirectionsCommand()
    {
        ShowTemporaryMessage("Calcul de l'itinéraire...");
        // TODO: Implémenter le calcul d'itinéraire
    }

    private void ExecuteZoomInCommand()
    {
        if (!_isMapInitialized || _currentRegion == null) return;

        try
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Réduire le rayon de moitié pour zoomer
                var newRadius = _currentRegion.Radius.Meters * 0.5;
                var newMapSpan = MapSpan.FromCenterAndRadius(_currentRegion.Center, Distance.FromMeters(Math.Max(newRadius, 100)));
                _mapControl.MoveToRegion(newMapSpan);
            });
            
            System.Diagnostics.Debug.WriteLine($"Zoom avant - Nouveau rayon: {_currentRegion.Radius.Meters * 0.5:F0}m");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur zoom avant: {ex.Message}");
        }
    }

    private void ExecuteZoomOutCommand()
    {
        if (!_isMapInitialized || _currentRegion == null) return;

        try
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Doubler le rayon pour dézoomer
                var newRadius = _currentRegion.Radius.Meters * 2.0;
                var newMapSpan = MapSpan.FromCenterAndRadius(_currentRegion.Center, Distance.FromMeters(Math.Min(newRadius, 100000))); // Max 100km
                _mapControl.MoveToRegion(newMapSpan);
            });
            
            System.Diagnostics.Debug.WriteLine($"Zoom arrière - Nouveau rayon: {_currentRegion.Radius.Meters * 2.0:F0}m");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur zoom arrière: {ex.Message}");
        }
    }

    #endregion

    #region Data Loading and Management

    private async Task LoadUserDataAsync()
    {
        try
        {
            var currentUserId = await _sessionService.GetCurrentUserIdAsync();
            if (currentUserId.HasValue)
            {
                // Charger les voyages de l'utilisateur
                var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(currentUserId.Value);
                _userVoyages = voyages?.ToList() ?? new List<Voyage>();
                
                System.Diagnostics.Debug.WriteLine($"Chargé {_userVoyages.Count} voyages pour l'utilisateur");
                
                // Charger les points d'intérêt si des voyages existent
                if (_userVoyages.Any())
                {
                    await LoadPointsOfInterestAsync();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Aucun utilisateur connecté");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des données: {ex.Message}");
        }
    }

    private async Task LoadPointsOfInterestAsync()
    {
        try
        {
            _accommodationPins.Clear();
            _activityPins.Clear();
            _restaurantPins.Clear();
            _transportPins.Clear();

            foreach (var voyage in _userVoyages)
            {
                // Charger les hébergements si la méthode existe
                if (voyage.Hebergements != null)
                {
                    foreach (var hebergement in voyage.Hebergements)
                    {
                        // Utiliser l'adresse pour le géocodage si les coordonnées ne sont pas disponibles
                        if (!string.IsNullOrEmpty(hebergement.Adresse))
                        {
                            System.Diagnostics.Debug.WriteLine($"Géocodage hébergement: {hebergement.Nom} - {hebergement.Adresse}");
                            var location = await GeocodeLocationAsync(hebergement.Adresse);
                            if (location != null)
                            {
                                var pin = new Pin
                                {
                                    Location = location,
                                    Label = $"🏨 {hebergement.Nom}",
                                    Address = hebergement.Adresse,
                                    Type = PinType.Place
                                };
                                _accommodationPins.Add(pin);
                                System.Diagnostics.Debug.WriteLine($"Pin hébergement ajouté: {hebergement.Nom} à {location.Latitude:F4}, {location.Longitude:F4}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Échec géocodage hébergement: {hebergement.Adresse}");
                            }

                            // Petit délai pour éviter de surcharger l'API de géocodage
                            await Task.Delay(500);
                        }
                    }
                }

                // Charger les activités si la méthode existe
                if (voyage.Activites != null)
                {
                    foreach (var activite in voyage.Activites)
                    {
                        // Utiliser la localisation pour le géocodage
                        if (!string.IsNullOrEmpty(activite.Localisation))
                        {
                            System.Diagnostics.Debug.WriteLine($"Géocodage activité: {activite.Nom} - {activite.Localisation}");
                            var location = await GeocodeLocationAsync(activite.Localisation);
                            if (location != null)
                            {
                                var pin = new Pin
                                {
                                    Location = location,
                                    Label = $"🎯 {activite.Nom}",
                                    Address = activite.Localisation,
                                    Type = PinType.Place
                                };
                                _activityPins.Add(pin);
                                System.Diagnostics.Debug.WriteLine($"Pin activité ajouté: {activite.Nom} à {location.Latitude:F4}, {location.Longitude:F4}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Échec géocodage activité: {activite.Localisation}");
                            }

                            // Petit délai pour éviter de surcharger l'API de géocodage
                            await Task.Delay(500);
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Chargé {_accommodationPins.Count} hébergements et {_activityPins.Count} activités");
            
            // Mettre à jour la carte si elle est initialisée
            if (_isMapInitialized)
            {
                await UpdateMapPins();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des points d'intérêt: {ex.Message}");
        }
    }

    public async Task RefreshDataAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            await LoadUserDataAsync();
            ShowTemporaryMessage("Données mises à jour");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du rafraîchissement: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de la mise à jour");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Map Pins Management

    private async Task UpdateMapPins()
    {
        if (!_isMapInitialized) return;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Supprimer tous les pins existants sauf les pins de recherche et utilisateur
                var pinsToRemove = _mapControl.Pins
                    .Where(p => p.Type != PinType.SearchResult && p.Type != PinType.Generic)
                    .ToList();
                
                foreach (var pin in pinsToRemove)
                {
                    _mapControl.Pins.Remove(pin);
                }

                // Ajouter les pins selon les filtres actifs
                if (ShowAccommodations)
                {
                    foreach (var pin in _accommodationPins)
                    {
                        _mapControl.Pins.Add(pin);
                    }
                }

                if (ShowActivities)
                {
                    foreach (var pin in _activityPins)
                    {
                        _mapControl.Pins.Add(pin);
                    }
                }

                if (ShowRestaurants)
                {
                    foreach (var pin in _restaurantPins)
                    {
                        _mapControl.Pins.Add(pin);
                    }
                }

                if (ShowTransport)
                {
                    foreach (var pin in _transportPins)
                    {
                        _mapControl.Pins.Add(pin);
                    }
                }

                var totalPins = _mapControl.Pins.Count;
                System.Diagnostics.Debug.WriteLine($"Pins mis à jour - Total visible: {totalPins}");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des pins: {ex.Message}");
        }
    }

    #endregion

    #region Location Services

    public async Task<Location> GeocodeLocationAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            System.Diagnostics.Debug.WriteLine($"Tentative de géocodage: {address}");
            
            var locations = await Geocoding.Default.GetLocationsAsync(address);
            var location = locations?.FirstOrDefault();
            
            if (location != null)
            {
                System.Diagnostics.Debug.WriteLine($"Géocodage réussi pour '{address}': {location.Latitude:F4}, {location.Longitude:F4}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Aucun résultat de géocodage pour '{address}'");
            }
            
            return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de géocodage pour '{address}': {ex.Message}");
            return null;
        }
    }

    public async Task<string> GetAddressFromLocationAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            
            if (placemark != null)
            {
                var address = $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, {placemark.Locality}, {placemark.CountryName}".Trim(' ', ',');
                System.Diagnostics.Debug.WriteLine($"Géocodage inverse réussi: {address}");
                return address;
            }
            
            return $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de géocodage inverse: {ex.Message}");
            return $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
    }

    public void ShowLocationDetails(Pin pin)
    {
        try
        {
            SelectedLocationName = pin.Label ?? "Lieu inconnu";
            SelectedLocationAddress = pin.Address ?? "";
            
            // Calculer la distance si position utilisateur disponible
            if (_userLocation != null && pin.Location != null)
            {
                var distance = Location.CalculateDistance(_userLocation, pin.Location, DistanceUnits.Kilometers);
                SelectedLocationDistance = distance < 1 
                    ? $"{distance * 1000:F0}m de vous" 
                    : $"{distance:F1}km de vous";
            }
            else
            {
                SelectedLocationDistance = "";
            }
            
            ShowLocationInfo = true;
            System.Diagnostics.Debug.WriteLine($"Détails affichés pour: {SelectedLocationName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage des détails: {ex.Message}");
        }
    }

    #endregion

    #region UI Helpers

    private async void ShowTemporaryMessage(string message, int durationMs = 3000)
    {
        try
        {
            // Annuler le message précédent s'il existe
            _messagesCancellationTokenSource?.Cancel();
            _messagesCancellationTokenSource = new CancellationTokenSource();
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessageText = message;
                ShowMessage = true;
            });
            
            // Masquer le message après le délai spécif
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class MapViewModel : INotifyPropertyChanged
{
    private readonly IVoyageService _voyageService;
    private readonly IActiviteService _activiteService;
    private readonly IHebergementService _hebergementService;
    private readonly ISessionService _sessionService;

    // Fix the ambiguous Map reference by using fully qualified name
    private Microsoft.Maui.Controls.Maps.Map _mapControl;
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
    
    // Collections pour les points d'intérêt
    private List<Pin> _accommodationPins = new();
    private List<Pin> _activityPins = new();
    private List<Pin> _restaurantPins = new();
    private List<Pin> _transportPins = new();

    // Données des voyages
    private List<Voyage> _userVoyages = new();

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
            UpdateMapPins();
        }
    }

    public bool ShowActivities
    {
        get => _showActivities;
        set
        {
            _showActivities = value;
            OnPropertyChanged();
            UpdateMapPins();
        }
    }

    public bool ShowRestaurants
    {
        get => _showRestaurants;
        set
        {
            _showRestaurants = value;
            OnPropertyChanged();
            UpdateMapPins();
        }
    }

    public bool ShowTransport
    {
        get => _showTransport;
        set
        {
            _showTransport = value;
            OnPropertyChanged();
            UpdateMapPins();
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

        SearchCommand = new Command(async () => await ExecuteSearchCommand());
        ToggleViewModeCommand = new Command(() => ExecuteToggleViewModeCommand());
        ToggleMapStyleCommand = new Command(() => ExecuteToggleMapStyleCommand());
        GoToMyLocationCommand = new Command(async () => await ExecuteGoToMyLocationCommand());
        ToggleFiltersCommand = new Command(() => ExecuteToggleFiltersCommand());
        CloseLocationInfoCommand = new Command(() => ExecuteCloseLocationInfoCommand());
        ShowAccommodationsCommand = new Command(() => ExecuteShowAccommodationsCommand());
        ShowActivitiesCommand = new Command(() => ExecuteShowActivitiesCommand());
        ShowRestaurantsCommand = new Command(() => ExecuteShowRestaurantsCommand());
        ShowDirectionsCommand = new Command(() => ExecuteShowDirectionsCommand());
        
        _ = LoadUserDataAsync();
    }

    public void SetMapControl(Microsoft.Maui.Controls.Maps.Map mapControl)
    {
        _mapControl = mapControl;
        
        // Configurer les événements de la carte
        if (_mapControl != null)
        {
            // CORRECTION: Supprimer les propriétés inexistantes et activer les interactions de base
            _mapControl.IsZoomEnabled = true;
            _mapControl.IsScrollEnabled = true;
            
            // S'assurer que le contrôle peut recevoir le focus pour les interactions
            _mapControl.InputTransparent = false;
            
            // S'abonner aux événements nécessaires
            SetupMapEvents();
        }
        
        UpdateMapPins();
    }
    
    private void SetupMapEvents()
    {
        if (_mapControl != null)
        {
            _mapControl.PropertyChanged += OnMapPropertyChanged;
            
            // Ajouter une gestion des événements de pins si nécessaire
            _mapControl.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_mapControl.Pins))
                {
                    // Configurer les événements pour chaque pin
                    foreach (var pin in _mapControl.Pins)
                    {
                        // Les pins MAUI peuvent déclencher des événements via leur propriété InfoWindowClicked
                        // mais cela dépend de la plateforme
                    }
                }
            };
        }
    }
    
    private void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Microsoft.Maui.Controls.Maps.Map.VisibleRegion))
        {
            System.Diagnostics.Debug.WriteLine("Région de la carte mise à jour");
        }
    }

    #region Command Implementations

    private async Task ExecuteSearchCommand()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || _mapControl == null)
            return;

        IsLoading = true;
        try
        {
            var searchResult = await GeocodeLocationAsync(SearchQuery);
            
            if (searchResult != null)
            {
                var mapSpan = MapSpan.FromCenterAndRadius(searchResult, Distance.FromKilometers(5));
                _mapControl.MoveToRegion(mapSpan);
                
                var searchPin = new Pin
                {
                    Location = searchResult,
                    Label = SearchQuery,
                    Address = "Résultat de recherche",
                    Type = PinType.SearchResult
                };
                
                var existingSearchPins = _mapControl.Pins.Where(p => p.Type == PinType.SearchResult).ToList();
                foreach (var pin in existingSearchPins)
                {
                    _mapControl.Pins.Remove(pin);
                }
                
                _mapControl.Pins.Add(searchPin);
                ShowTemporaryMessage($"Lieu trouvé: {SearchQuery}");
                ShowLocationDetails(searchPin);
            }
            else
            {
                ShowTemporaryMessage("Aucun résultat trouvé");
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

    private void ExecuteToggleViewModeCommand()
    {
        if (_mapControl == null) return;

        switch (_mapControl.MapType)
        {
            case MapType.Street:
                _mapControl.MapType = MapType.Satellite;
                ViewModeIcon = "🛰️";
                break;
            case MapType.Satellite:
                _mapControl.MapType = MapType.Hybrid;
                ViewModeIcon = "🌍";
                break;
            case MapType.Hybrid:
                _mapControl.MapType = MapType.Street;
                ViewModeIcon = "🗺️";
                break;
        }
        
        var modeText = _mapControl.MapType switch
        {
            MapType.Street => "Plan",
            MapType.Satellite => "Satellite",
            MapType.Hybrid => "Hybride",
            _ => "Plan"
        };
        
        ShowTemporaryMessage($"Mode: {modeText}");
    }

    private void ExecuteToggleMapStyleCommand()
    {
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        
        if (currentTheme == AppTheme.Light)
        {
            MapStyleIcon = "☀️";
            ShowTemporaryMessage("Thème sombre activé");
        }
        else
        {
            MapStyleIcon = "🌙";
            ShowTemporaryMessage("Thème clair activé");
        }
    }

    private async Task ExecuteGoToMyLocationCommand()
    {
        if (_mapControl == null) return;

        IsLoading = true;
        try
        {
            var location = await GetCurrentLocationAsync();
            if (location != null)
            {
                _userLocation = location;
                
                var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1));
                _mapControl.MoveToRegion(mapSpan);
                
                var userPin = _mapControl.Pins.FirstOrDefault(p => p.Label == "Ma position");
                if (userPin != null)
                {
                    _mapControl.Pins.Remove(userPin);
                }
                
                var newUserPin = new Pin
                {
                    Location = location,
                    Label = "Ma position",
                    Address = "Position actuelle",
                    Type = PinType.Generic
                };
                
                _mapControl.Pins.Add(newUserPin);
                ShowTemporaryMessage("Position actuelle localisée");
            }
            else
            {
                ShowTemporaryMessage("Impossible d'obtenir votre position");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de géolocalisation: {ex.Message}");
            ShowTemporaryMessage("Erreur de géolocalisation");
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
            ShowTemporaryMessage("Filtres ouverts");
        }
        else
        {
            ShowTemporaryMessage("Filtres fermés");
        }
    }

    private void ExecuteCloseLocationInfoCommand()
    {
        ShowLocationInfo = false;
    }

    private void ExecuteShowAccommodationsCommand()
    {
        ShowLocationInfo = false;
        ShowAccommodations = true;
        ShowRestaurants = false;
        ShowActivities = false;
        ShowTransport = false;
        ShowTemporaryMessage("Hébergements affichés");
    }

    private void ExecuteShowActivitiesCommand()
    {
        ShowLocationInfo = false;
        ShowAccommodations = false;
        ShowRestaurants = false;
        ShowActivities = true;
        ShowTransport = false;
        ShowTemporaryMessage("Activités affichées");
    }

    private void ExecuteShowRestaurantsCommand()
    {
        ShowLocationInfo = false;
        ShowAccommodations = false;
        ShowRestaurants = true;
        ShowActivities = false;
        ShowTransport = false;
        ShowTemporaryMessage("Restaurants affichés");
    }

    private void ExecuteShowDirectionsCommand()
    {
        ShowLocationInfo = false;
        ShowTemporaryMessage("Calcul d'itinéraire...");
    }

    #endregion

    #region Private Methods

    private async Task LoadUserDataAsync()
    {
        try
        {
            var currentUserId = await _sessionService.GetCurrentUserIdAsync();
            if (currentUserId.HasValue)
            {
                if (_voyageService != null)
                {
                    try 
                    {
                        var getUserVoyagesMethod = _voyageService.GetType().GetMethod("GetVoyagesByUtilisateurAsync");
                        if (getUserVoyagesMethod != null)
                        {
                            var task = (Task<List<Voyage>>)getUserVoyagesMethod.Invoke(_voyageService, new object[] { currentUserId.Value });
                            _userVoyages = await task ?? new List<Voyage>();
                        }
                        else
                        {
                            _userVoyages = new List<Voyage>();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des voyages: {ex.Message}");
                        _userVoyages = new List<Voyage>();
                    }
                }
                
                await LoadPinsFromUserDataAsync();
            }
            else
            {
                await LoadPinsFromUserDataAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des données utilisateur: {ex.Message}");
            await LoadPinsFromUserDataAsync();
        }
    }

    private async Task LoadPinsFromUserDataAsync()
    {
        try
        {
            _accommodationPins.Clear();
            _activityPins.Clear();

            // CORRECTION: Chargement amélioré des hébergements
            var hebergements = await _hebergementService.GetAllHebergementsAsync(); 
            foreach (var hebergement in hebergements)
            {
                Location location = null;
                string displayAddress = "";
                
                // Priorité 1: Utiliser l'adresse si elle existe
                if (!string.IsNullOrEmpty(hebergement.Adresse))
                {
                    location = await GeocodeLocationAsync(hebergement.Adresse);
                    displayAddress = hebergement.Adresse;
                }
                
                // Priorité 2: Utiliser le nom pour le géocodage
                if (location == null && !string.IsNullOrEmpty(hebergement.Nom))
                {
                    location = await GeocodeLocationAsync(hebergement.Nom);
                    if (string.IsNullOrEmpty(displayAddress))
                    {
                        displayAddress = hebergement.Nom;
                    }
                }
                
                // Priorité 3: Position par défaut avec info du type
                if (location == null)
                {
                    location = GetRandomLocationAroundParis();
                    if (string.IsNullOrEmpty(displayAddress))
                    {
                        displayAddress = hebergement.TypeHebergement ?? "Hébergement";
                    }
                }
                
                // Créer une adresse complète pour l'affichage
                var fullAddress = BuildFullAddress(hebergement.Adresse, hebergement.TypeHebergement);
                
                var pin = new Pin
                {
                    Location = location,
                    Label = hebergement.Nom ?? "Hébergement",
                    Address = fullAddress,
                    Type = PinType.Place
                };
                
                _accommodationPins.Add(pin);
                System.Diagnostics.Debug.WriteLine($"Hébergement ajouté: {pin.Label} - {pin.Address}");
            }

            // CORRECTION: Chargement amélioré des activités
            var activites = await _activiteService.GetAllActivitesAsync();
            foreach (var activite in activites)
            {
                Location location = null;
                string displayAddress = "";
                
                // Priorité 1: Utiliser la localisation si elle existe
                if (!string.IsNullOrEmpty(activite.Localisation))
                {
                    location = await GeocodeLocationAsync(activite.Localisation);
                    displayAddress = activite.Localisation;
                }
                
                // Priorité 2: Utiliser le nom pour le géocodage
                if (location == null && !string.IsNullOrEmpty(activite.Nom))
                {
                    location = await GeocodeLocationAsync(activite.Nom);
                    if (string.IsNullOrEmpty(displayAddress))
                    {
                        displayAddress = activite.Nom;
                    }
                }
                
                // Priorité 3: Position par défaut
                if (location == null)
                {
                    location = GetRandomLocationAroundParis();
                    if (string.IsNullOrEmpty(displayAddress))
                    {
                        displayAddress = "Activité";
                    }
                }
                
                // Créer une adresse complète pour l'affichage
                var fullAddress = BuildFullAddress(activite.Localisation, activite.Description);
                
                var pin = new Pin
                {
                    Location = location,
                    Label = activite.Nom ?? "Activité",
                    Address = fullAddress,
                    Type = PinType.Place
                };
                
                _activityPins.Add(pin);
                System.Diagnostics.Debug.WriteLine($"Activité ajoutée: {pin.Label} - {pin.Address}");
            }

            LoadSampleRestaurantAndTransportPins();
            UpdateMapPins();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des pins: {ex.Message}");
            LoadSampleRestaurantAndTransportPins();
            UpdateMapPins();
        }
    }

    // NOUVELLE MÉTHODE: Construire une adresse complète pour l'affichage
    private string BuildFullAddress(string primaryAddress, string secondaryInfo)
    {
        var addressParts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(primaryAddress))
        {
            addressParts.Add(primaryAddress.Trim());
        }
        
        if (!string.IsNullOrWhiteSpace(secondaryInfo) && 
            secondaryInfo.Trim() != primaryAddress?.Trim())
        {
            addressParts.Add(secondaryInfo.Trim());
        }
        
        return addressParts.Count > 0 ? string.Join(" - ", addressParts) : "Adresse non disponible";
    }

    private Location GetRandomLocationAroundParis()
    {
        var random = new Random();
        var parisLat = 48.8566;
        var parisLon = 2.3522;
        
        var latOffset = (random.NextDouble() - 0.5) * 0.4;
        var lonOffset = (random.NextDouble() - 0.5) * 0.4;
        
        return new Location(parisLat + latOffset, parisLon + lonOffset);
    }

    private void LoadSampleRestaurantAndTransportPins()
    {
        _restaurantPins = new List<Pin>
        {
            new Pin
            {
                Location = new Location(48.8566, 2.3522),
                Label = "Le Grand Véfour",
                Address = "17 Rue de Beaujolais, Paris - Restaurant gastronomique",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8606, 2.3376),
                Label = "L'Ami Jean",
                Address = "27 Rue Malar, Paris - Cuisine traditionnelle",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8584, 2.2945),
                Label = "Le Jules Verne",
                Address = "Tour Eiffel, Paris - Restaurant avec vue",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8738, 2.3522),
                Label = "Chez Prune",
                Address = "36 Rue Beaurepaire, Paris - Café-restaurant",
                Type = PinType.Place
            }
        };

        _transportPins = new List<Pin>
        {
            new Pin
            {
                Location = new Location(48.8798, 2.3554),
                Label = "Gare du Nord",
                Address = "18 Rue de Dunkerque, Paris - Gare SNCF",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8449, 2.3738),
                Label = "Gare de Lyon",
                Address = "20 Boulevard Diderot, Paris - Gare SNCF",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8424, 2.3226),
                Label = "Gare Montparnasse",
                Address = "17 Boulevard de Vaugirard, Paris - Gare SNCF",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8768, 2.2584),
                Label = "Métro Charles de Gaulle",
                Address = "Place Charles de Gaulle, Paris - Station de métro",
                Type = PinType.Place
            }
        };
    }

    private void UpdateMapPins()
    {
        if (_mapControl == null) return;

        try
        {
            var specialPins = _mapControl.Pins.Where(p => 
                p.Type == PinType.SearchResult || 
                p.Label == "Ma position").ToList();

            _mapControl.Pins.Clear();

            foreach (var pin in specialPins)
            {
                _mapControl.Pins.Add(pin);
            }

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

            var activeFilters = new List<string>();
            if (ShowAccommodations) activeFilters.Add("Hébergements");
            if (ShowActivities) activeFilters.Add("Activités");
            if (ShowRestaurants) activeFilters.Add("Restaurants");
            if (ShowTransport) activeFilters.Add("Transports");

            System.Diagnostics.Debug.WriteLine($"Carte mise à jour avec {_mapControl.Pins.Count} pins. Filtres actifs: {string.Join(", ", activeFilters)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des pins: {ex.Message}");
        }
    }

    private async Task<Location> GeocodeLocationAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            var locations = await Geocoding.Default.GetLocationsAsync(address);
            var location = locations?.FirstOrDefault();
            
            if (location != null)
            {
                System.Diagnostics.Debug.WriteLine($"Géocodage réussi pour '{address}': {location.Latitude}, {location.Longitude}");
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

    private async Task<Location> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("Permission de géolocalisation refusée");
                return null;
            }

            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            };

            var location = await Geolocation.Default.GetLocationAsync(request);
            
            if (location != null)
            {
                System.Diagnostics.Debug.WriteLine($"Position obtenue: {location.Latitude}, {location.Longitude}");
            }
            
            return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de géolocalisation: {ex.Message}");
            return null;
        }
    }

    private async void ShowTemporaryMessage(string message)
    {
        try
        {
            MessageText = message;
            ShowMessage = true;

            await Task.Delay(3000);
            
            if (MessageText == message)
            {
                ShowMessage = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage du message: {ex.Message}");
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Public Methods

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        try
        {
            await LoadUserDataAsync();
            ShowTemporaryMessage("Données actualisées");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'actualisation: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de l'actualisation");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ShowLocationDetails(Pin pin)
    {
        if (pin != null)
        {
            SelectedLocationName = pin.Label;
            SelectedLocationAddress = pin.Address;
            
            if (_userLocation != null)
            {
                var distance = Location.CalculateDistance(_userLocation, pin.Location, DistanceUnits.Kilometers);
                SelectedLocationDistance = $"À {distance:F1} km";
            }
            else
            {
                SelectedLocationDistance = "";
            }

            ShowLocationInfo = true;
            System.Diagnostics.Debug.WriteLine($"Affichage des détails pour: {pin.Label} - {pin.Address}");
        }
    }

    public void CenterMapOnLocation(Location location, double radiusKm = 5)
    {
        if (_mapControl != null && location != null)
        {
            try
            {
                var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(radiusKm));
                _mapControl.MoveToRegion(mapSpan);
                System.Diagnostics.Debug.WriteLine($"Carte centrée sur: {location.Latitude}, {location.Longitude}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du centrage: {ex.Message}");
            }
        }
    }

    public void HandlePinTapped(Pin tappedPin)
    {
        if (tappedPin != null)
        {
            ShowLocationDetails(tappedPin);
            CenterMapOnLocation(tappedPin.Location, 2);
        }
    }

    public void ZoomIn()
    {
        if (_mapControl?.VisibleRegion != null)
        {
            try
            {
                var currentRegion = _mapControl.VisibleRegion;
                var center = currentRegion.Center;
                var newRadius = Distance.FromMeters(currentRegion.Radius.Meters * 0.5); // Zoom x2
                
                var newSpan = MapSpan.FromCenterAndRadius(center, newRadius);
                _mapControl.MoveToRegion(newSpan);
                
                System.Diagnostics.Debug.WriteLine($"Zoom avant effectué - Nouveau rayon: {newRadius.Meters}m");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du zoom avant: {ex.Message}");
            }
        }
    }

    public void ZoomOut()
    {
        if (_mapControl?.VisibleRegion != null)
        {
            try
            {
                var currentRegion = _mapControl.VisibleRegion;
                var center = currentRegion.Center;
                var newRadius = Distance.FromMeters(currentRegion.Radius.Meters * 2.0); // Zoom /2
                
                var newSpan = MapSpan.FromCenterAndRadius(center, newRadius);
                _mapControl.MoveToRegion(newSpan);
                
                System.Diagnostics.Debug.WriteLine($"Zoom arrière effectué - Nouveau rayon: {newRadius.Meters}m");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du zoom arrière: {ex.Message}");
            }
        }
    }

    public async Task<bool> AddCustomLocationAsync(string locationName, string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(locationName) || string.IsNullOrWhiteSpace(address))
                return false;

            var location = await GeocodeLocationAsync(address);
            if (location == null)
                return false;

            var customPin = new Pin
            {
                Location = location,
                Label = locationName,
                Address = address,
                Type = PinType.Generic
            };

            _mapControl?.Pins.Add(customPin);
            ShowLocationDetails(customPin);
            CenterMapOnLocation(location, 2);
            ShowTemporaryMessage($"Lieu ajouté: {locationName}");
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout du lieu personnalisé: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de l'ajout du lieu");
            return false;
        }
    }

    public void RemoveCustomPins()
    {
        if (_mapControl == null) return;

        try
        {
            var customPins = _mapControl.Pins
                .Where(p => p.Type == PinType.Generic && p.Label != "Ma position")
                .ToList();

            foreach (var pin in customPins)
            {
                _mapControl.Pins.Remove(pin);
            }

            if (customPins.Count > 0)
            {
                ShowTemporaryMessage($"{customPins.Count} lieu(x) personnalisé(s) supprimé(s)");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression des lieux personnalisés: {ex.Message}");
        }
    }

    public async Task<List<Pin>> GetNearbyPinsAsync(Location center, double radiusKm = 5)
    {
        try
        {
            if (center == null || _mapControl == null)
                return new List<Pin>();

            var nearbyPins = new List<Pin>();

            foreach (var pin in _mapControl.Pins)
            {
                var distance = Location.CalculateDistance(center, pin.Location, DistanceUnits.Kilometers);
                if (distance <= radiusKm)
                {
                    nearbyPins.Add(pin);
                }
            }

            return nearbyPins.OrderBy(p => Location.CalculateDistance(center, p.Location, DistanceUnits.Kilometers)).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la recherche de lieux proches: {ex.Message}");
            return new List<Pin>();
        }
    }

    public async Task ExportMapDataAsync()
    {
        try
        {
            if (_mapControl?.Pins == null || !_mapControl.Pins.Any())
            {
                ShowTemporaryMessage("Aucune donnée à exporter");
                return;
            }

            var exportData = new
            {
                ExportDate = DateTime.Now,
                UserLocation = _userLocation != null ? new { _userLocation.Latitude, _userLocation.Longitude } : null,
                TotalPins = _mapControl.Pins.Count,
                Pins = _mapControl.Pins.Select(p => new
                {
                    p.Label,
                    p.Address,
                    Location = new { p.Location.Latitude, p.Location.Longitude },
                    Type = p.Type.ToString()
                }).ToList(),
                Filters = new
                {
                    ShowAccommodations,
                    ShowActivities,
                    ShowRestaurants,
                    ShowTransport
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.Diagnostics.Debug.WriteLine($"Données exportées: {json}");
            ShowTemporaryMessage("Données exportées avec succès");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'export: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de l'export");
        }
    }

    public void ResetMapToDefault()
    {
        try
        {
            // Position par défaut : Paris
            var defaultLocation = new Location(48.8566, 2.3522);
            var defaultSpan = MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(10));
            
            _mapControl?.MoveToRegion(defaultSpan);
            
            // Réinitialiser les filtres
            ShowAccommodations = true;
            ShowActivities = true;
            ShowRestaurants = false;
            ShowTransport = false;
            
            // Fermer les panneaux
            ShowLocationInfo = false;
            ShowFilters = false;
            
            // Nettoyer les pins de recherche et personnalisés
            if (_mapControl != null)
            {
                var pinsToRemove = _mapControl.Pins
                    .Where(p => p.Type == PinType.SearchResult || 
                               (p.Type == PinType.Generic && p.Label != "Ma position"))
                    .ToList();
                
                foreach (var pin in pinsToRemove)
                {
                    _mapControl.Pins.Remove(pin);
                }
            }
            
            // Réinitialiser le style de carte
            if (_mapControl != null)
            {
                _mapControl.MapType = MapType.Street;
                ViewModeIcon = "🗺️";
            }
            
            ShowTemporaryMessage("Carte réinitialisée");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la réinitialisation: {ex.Message}");
            ShowTemporaryMessage("Erreur lors de la réinitialisation");
        }
    }

    public void UpdateMapRegion(MapSpan newRegion)
    {
        try
        {
            if (_mapControl != null && newRegion != null)
            {
                _mapControl.MoveToRegion(newRegion);
                System.Diagnostics.Debug.WriteLine($"Région mise à jour - Centre: {newRegion.Center.Latitude}, {newRegion.Center.Longitude}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour de la région: {ex.Message}");
        }
    }

    public async Task<string> GetAddressFromLocationAsync(Location location)
    {
        try
        {
            if (location == null)
                return "Adresse inconnue";

            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            
            if (placemark != null)
            {
                var addressParts = new List<string>();
                
                if (!string.IsNullOrEmpty(placemark.Thoroughfare))
                    addressParts.Add(placemark.Thoroughfare);
                if (!string.IsNullOrEmpty(placemark.Locality))
                    addressParts.Add(placemark.Locality);
                if (!string.IsNullOrEmpty(placemark.AdminArea))
                    addressParts.Add(placemark.AdminArea);
                if (!string.IsNullOrEmpty(placemark.CountryName))
                    addressParts.Add(placemark.CountryName);
                
                return addressParts.Count > 0 ? string.Join(", ", addressParts) : "Adresse inconnue";
            }
            
            return $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du géocodage inverse: {ex.Message}");
            return $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
    }

    public void Dispose()
    {
        try
        {
            // Nettoyer les événements
            if (_mapControl != null)
            {
                _mapControl.PropertyChanged -= OnMapPropertyChanged;
            }
            
            // Nettoyer les collections
            _accommodationPins?.Clear();
            _activityPins?.Clear();
            _restaurantPins?.Clear();
            _transportPins?.Clear();
            _userVoyages?.Clear();
            
            System.Diagnostics.Debug.WriteLine("MapViewModel nettoyé");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage du MapViewModel: {ex.Message}");
        }
    }

    #endregion

    #region Event Handlers

    private void OnLocationChanged(object sender, LocationEventArgs e)
    {
        try
        {
            if (e.Location != null)
            {
                _userLocation = e.Location;
                System.Diagnostics.Debug.WriteLine($"Position utilisateur mise à jour: {e.Location.Latitude}, {e.Location.Longitude}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour de la position: {ex.Message}");
        }
    }

    #endregion
}

// Classe d'arguments pour les événements de localisation
public class LocationEventArgs : EventArgs
{
    public Location Location { get; }
    
    public LocationEventArgs(Location location)
    {
        Location = location;
    }
}
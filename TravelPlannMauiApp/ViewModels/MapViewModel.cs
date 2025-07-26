using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using BU.Services;
using DAL.DB; // Add this using directive for Voyage model

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

    // Données des voyages - Now properly typed
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
            // S'abonner aux événements nécessaires
            SetupMapEvents();
        }
        
        UpdateMapPins();
    }
    
    private void SetupMapEvents()
    {
        // Événement pour détecter quand un pin est tapé
        // Note: Cet événement n'existe pas directement sur Map, 
        // il devra être géré dans le code-behind de la page
    }

    #region Command Implementations

    private async Task ExecuteSearchCommand()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || _mapControl == null)
            return;

        IsLoading = true;
        try
        {
            // Simuler une recherche géocodée
            await Task.Delay(1000); // Simulation d'API call
            
            var searchResult = await GeocodeLocationAsync(SearchQuery);
            
            if (searchResult != null)
            {
                // Centrer la carte sur le résultat
                var mapSpan = MapSpan.FromCenterAndRadius(searchResult, Distance.FromKilometers(5));
                _mapControl.MoveToRegion(mapSpan);
                
                // Ajouter un pin pour le résultat
                var pin = new Pin
                {
                    Location = searchResult,
                    Label = SearchQuery,
                    Address = "Résultat de recherche",
                    Type = PinType.SearchResult
                };
                
                _mapControl.Pins.Add(pin);
                
                ShowTemporaryMessage($"Lieu trouvé: {SearchQuery}");
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
                ViewModeIcon = "🗺️";
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
        // Basculer entre thème clair et sombre
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
                var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(2));
                _mapControl.MoveToRegion(mapSpan);
                
                ShowTemporaryMessage("Position actuelle");
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
        ShowTemporaryMessage("Itinéraire calculé");
        // Ici vous pouvez implémenter la logique d'itinéraire
    }

    #endregion

    #region Private Methods

    private async Task LoadUserDataAsync()
    {
        try
        {
            var currentUser = _sessionService.GetCurrentUser();
            if (currentUser != null)
            {
                _userVoyages = await _voyageService.GetVoyagesByUtilisateurAsync(currentUser.Id);
                await LoadPinsFromUserDataAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des données utilisateur: {ex.Message}");
        }
    }

    private async Task LoadPinsFromUserDataAsync()
    {
        try
        {
            _accommodationPins.Clear();
            _activityPins.Clear();

            foreach (var voyage in _userVoyages)
            {
                // Charger les hébergements du voyage
                var hebergements = await _hebergementService.GetHebergementsByVoyageAsync(voyage.Id);
                foreach (var hebergement in hebergements)
                {
                    if (!string.IsNullOrEmpty(hebergement.Adresse))
                    {
                        var location = await GeocodeLocationAsync(hebergement.Adresse);
                        if (location != null)
                        {
                            var pin = new Pin
                            {
                                Location = location,
                                Label = hebergement.Nom,
                                Address = hebergement.Adresse,
                                Type = PinType.Place
                            };
                            _accommodationPins.Add(pin);
                        }
                    }
                }

                // Charger les activités du voyage
                var activites = await _activiteService.GetActivitesByVoyageAsync(voyage.Id);
                foreach (var activite in activites)
                {
                    if (!string.IsNullOrEmpty(activite.Localisation))
                    {
                        var location = await GeocodeLocationAsync(activite.Localisation);
                        if (location != null)
                        {
                            var pin = new Pin
                            {
                                Location = location,
                                Label = activite.Nom,
                                Address = activite.Localisation,
                                Type = PinType.Place
                            };
                            _activityPins.Add(pin);
                        }
                    }
                }
            }

            // Charger des données de démonstration pour les restaurants et transports
            LoadSampleRestaurantAndTransportPins();

            UpdateMapPins();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des pins: {ex.Message}");
        }
    }

    private void LoadSampleRestaurantAndTransportPins()
    {
        // Données de démonstration pour les restaurants
        _restaurantPins = new List<Pin>
        {
            new Pin
            {
                Location = new Location(48.8566, 2.3522), // Paris
                Label = "Le Grand Véfour",
                Address = "17 Rue de Beaujolais, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8606, 2.3376), // Paris
                Label = "L'Ami Jean",
                Address = "27 Rue Malar, Paris",
                Type = PinType.Place
            }
        };

        // Données de démonstration pour les transports
        _transportPins = new List<Pin>
        {
            new Pin
            {
                Location = new Location(48.8738, 2.2950), // Gare du Nord
                Label = "Gare du Nord",
                Address = "18 Rue de Dunkerque, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.844, 2.3737), // Gare de Lyon
                Label = "Gare de Lyon",
                Address = "20 Boulevard Diderot, Paris",
                Type = PinType.Place
            }
        };
    }

    private void UpdateMapPins()
    {
        if (_mapControl == null) return;

        _mapControl.Pins.Clear();

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
    }

    private async Task<Location> GeocodeLocationAsync(string address)
    {
        try
        {
            var locations = await Geocoding.Default.GetLocationsAsync(address);
            return locations?.FirstOrDefault();
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
            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            };

            var location = await Geolocation.Default.GetLocationAsync(request);
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
        MessageText = message;
        ShowMessage = true;

        // Masquer le message après 3 secondes
        await Task.Delay(3000);
        ShowMessage = false;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Public Methods

    public async Task RefreshDataAsync()
    {
        await LoadUserDataAsync();
    }

    public void ShowLocationDetails(Pin pin)
    {
        if (pin != null)
        {
            SelectedLocationName = pin.Label;
            SelectedLocationAddress = pin.Address;
            
            // Calculer la distance si la position utilisateur est disponible
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
        }
    }

    public void CenterMapOnLocation(Location location, double radiusKm = 5)
    {
        if (_mapControl != null && location != null)
        {
            var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(radiusKm));
            _mapControl.MoveToRegion(mapSpan);
        }
    }

    public void HandlePinTapped(Pin tappedPin)
    {
        if (tappedPin != null)
        {
            ShowLocationDetails(tappedPin);
        }
    }

    #endregion
}
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
            // Activer les interactions utilisateur pour le zoom et le déplacement
            _mapControl.IsZoomEnabled = true;
            _mapControl.IsScrollEnabled = true;
            
            // S'abonner aux événements nécessaires
            SetupMapEvents();
        }
        
        UpdateMapPins();
    }
    
    private void SetupMapEvents()
    {
        // Événement pour détecter quand un pin est tapé
        // Note: Dans MAUI, l'événement PinClicked n'existe pas directement sur Map
        // Nous utiliserons une approche via les pins individuels dans le code-behind
        
        // Configuration pour permettre les interactions tactiles/souris
        if (_mapControl != null)
        {
            _mapControl.PropertyChanged += OnMapPropertyChanged;
        }
    }
    
    private void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Gérer les changements de propriétés de la carte si nécessaire
        if (e.PropertyName == nameof(Microsoft.Maui.Controls.Maps.Map.VisibleRegion))
        {
            // La région visible a changé (zoom ou déplacement)
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
            // Recherche géocodée
            var searchResult = await GeocodeLocationAsync(SearchQuery);
            
            if (searchResult != null)
            {
                // Centrer la carte sur le résultat avec un zoom approprié
                var mapSpan = MapSpan.FromCenterAndRadius(searchResult, Distance.FromKilometers(5));
                _mapControl.MoveToRegion(mapSpan);
                
                // Ajouter un pin temporaire pour le résultat de recherche
                var searchPin = new Pin
                {
                    Location = searchResult,
                    Label = SearchQuery,
                    Address = "Résultat de recherche",
                    Type = PinType.SearchResult
                };
                
                // Supprimer les anciens pins de recherche
                var existingSearchPins = _mapControl.Pins.Where(p => p.Type == PinType.SearchResult).ToList();
                foreach (var pin in existingSearchPins)
                {
                    _mapControl.Pins.Remove(pin);
                }
                
                _mapControl.Pins.Add(searchPin);
                ShowTemporaryMessage($"Lieu trouvé: {SearchQuery}");
                
                // Afficher les détails du lieu trouvé
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
                
                // Centrer la carte sur la position utilisateur avec un zoom proche
                var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1));
                _mapControl.MoveToRegion(mapSpan);
                
                // Ajouter ou mettre à jour le pin de position utilisateur
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
        // Ici vous pouvez implémenter la logique d'itinéraire
        // Par exemple, ouvrir l'app de navigation par défaut
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
                // Essayer de charger les voyages de l'utilisateur si la méthode existe
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
                // Charger des données par défaut si aucun utilisateur connecté
                await LoadPinsFromUserDataAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des données utilisateur: {ex.Message}");
            // Charger quand même les données par défaut
            await LoadPinsFromUserDataAsync();
        }
    }

    private async Task LoadPinsFromUserDataAsync()
    {
        try
        {
            _accommodationPins.Clear();
            _activityPins.Clear();

            // Charger les hébergements depuis la base de données
            var hebergements = await _hebergementService.GetAllHebergementsAsync(); 
            foreach (var hebergement in hebergements)
            {
                Location location = null;
                
                // Essayer d'abord avec l'adresse si elle existe (après migration)
                if (!string.IsNullOrEmpty(hebergement.Adresse))
                {
                    location = await GeocodeLocationAsync(hebergement.Adresse);
                }
                
                // Fallback: utiliser le nom pour le géocodage
                if (location == null && !string.IsNullOrEmpty(hebergement.Nom))
                {
                    location = await GeocodeLocationAsync(hebergement.Nom);
                }
                
                // Dernière option: position par défaut
                if (location == null)
                {
                    location = GetRandomLocationAroundParis();
                }
                
                var pin = new Pin
                {
                    Location = location,
                    Label = hebergement.Nom,
                    Address = hebergement.Adresse ?? hebergement.TypeHebergement ?? "Hébergement",
                    Type = PinType.Place
                };
                
                _accommodationPins.Add(pin);
            }

            // Charger les activités depuis la base de données
            var activites = await _activiteService.GetAllActivitesAsync();
            foreach (var activite in activites)
            {
                Location location = null;
                
                // Essayer d'abord avec la localisation si elle existe (après migration)
                if (!string.IsNullOrEmpty(activite.Localisation))
                {
                    location = await GeocodeLocationAsync(activite.Localisation);
                }
                
                // Fallback: utiliser le nom pour le géocodage
                if (location == null && !string.IsNullOrEmpty(activite.Nom))
                {
                    location = await GeocodeLocationAsync(activite.Nom);
                }
                
                // Dernière option: position par défaut
                if (location == null)
                {
                    location = GetRandomLocationAroundParis();
                }
                
                var pin = new Pin
                {
                    Location = location,
                    Label = activite.Nom,
                    Address = activite.Localisation ?? activite.Description ?? "Activité",
                    Type = PinType.Place
                };
                
                _activityPins.Add(pin);
            }

            // Charger des données de démonstration pour les restaurants et transports
            LoadSampleRestaurantAndTransportPins();

            UpdateMapPins();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des pins: {ex.Message}");
            
            // En cas d'erreur, charger au moins les données d'exemple
            LoadSampleRestaurantAndTransportPins();
            UpdateMapPins();
        }
    }

    private Location GetRandomLocationAroundParis()
    {
        // Génère une position aléatoire autour de Paris (dans un rayon de 20km)
        var random = new Random();
        var parisLat = 48.8566;
        var parisLon = 2.3522;
        
        // Décalage aléatoire (approximativement 20km de rayon)
        var latOffset = (random.NextDouble() - 0.5) * 0.4; // ±0.2 degrés
        var lonOffset = (random.NextDouble() - 0.5) * 0.4; // ±0.2 degrés
        
        return new Location(parisLat + latOffset, parisLon + lonOffset);
    }

    private void LoadSampleRestaurantAndTransportPins()
    {
        // Données de démonstration pour les restaurants avec plus de variété
        _restaurantPins = new List<Pin>
        {
            new Pin
            {
                Location = new Location(48.8566, 2.3522), // Paris Centre
                Label = "Le Grand Véfour",
                Address = "17 Rue de Beaujolais, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8606, 2.3376), // Paris 7e
                Label = "L'Ami Jean",
                Address = "27 Rue Malar, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8584, 2.2945), // Paris 16e
                Label = "Le Jules Verne",
                Address = "Tour Eiffel, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8738, 2.3522), // Paris 10e
                Label = "Chez Prune",
                Address = "36 Rue Beaurepaire, Paris",
                Type = PinType.Place
            }
        };

        // Données de démonstration pour les transports
        _transportPins = new List<Pin>
        {
            new Pin
            {
                Location = new Location(48.8798, 2.3554), // Gare du Nord
                Label = "Gare du Nord",
                Address = "18 Rue de Dunkerque, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8449, 2.3738), // Gare de Lyon
                Label = "Gare de Lyon",
                Address = "20 Boulevard Diderot, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8424, 2.3226), // Gare Montparnasse
                Label = "Gare Montparnasse",
                Address = "17 Boulevard de Vaugirard, Paris",
                Type = PinType.Place
            },
            new Pin
            {
                Location = new Location(48.8768, 2.2584), // Arc de Triomphe
                Label = "Métro Charles de Gaulle",
                Address = "Place Charles de Gaulle, Paris",
                Type = PinType.Place
            }
        };
    }

    private void UpdateMapPins()
    {
        if (_mapControl == null) return;

        try
        {
            // Conserver les pins spéciaux (recherche, position utilisateur)
            var specialPins = _mapControl.Pins.Where(p => 
                p.Type == PinType.SearchResult || 
                p.Label == "Ma position").ToList();

            // Nettoyer la carte
            _mapControl.Pins.Clear();

            // Rétablir les pins spéciaux
            foreach (var pin in specialPins)
            {
                _mapControl.Pins.Add(pin);
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

            System.Diagnostics.Debug.WriteLine($"Carte mise à jour avec {_mapControl.Pins.Count} pins");
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

            // Masquer le message après 3 secondes
            await Task.Delay(3000);
            
            // Vérifier que c'est toujours le même message avant de le masquer
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
            System.Diagnostics.Debug.WriteLine($"Affichage des détails pour: {pin.Label}");
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
            
            // Optionnel: centrer légèrement la carte sur le pin sélectionné
            CenterMapOnLocation(tappedPin.Location, 2);
        }
    }

    // Méthodes utilitaires pour les interactions avec la carte
    public void ZoomIn()
    {
        if (_mapControl?.VisibleRegion != null)
        {
            var region = _mapControl.VisibleRegion;
            var newRadius = Distance.FromKilometers(region.Radius.Kilometers * 0.5);
            var newSpan = MapSpan.FromCenterAndRadius(region.Center, newRadius);
            _mapControl.MoveToRegion(newSpan);
        }
    }

    public void ZoomOut()
    {
        if (_mapControl?.VisibleRegion != null)
        {
            var region = _mapControl.VisibleRegion;
            var newRadius = Distance.FromKilometers(region.Radius.Kilometers * 2.0);
            var newSpan = MapSpan.FromCenterAndRadius(region.Center, newRadius);
            _mapControl.MoveToRegion(newSpan);
        }
    }

    public void ResetMapView()
    {
        // Retourner à la vue par défaut (Paris)
        var parisLocation = new Location(48.8566, 2.3522);
        CenterMapOnLocation(parisLocation, 10);
        ShowTemporaryMessage("Vue par défaut");
    }

    #endregion
}
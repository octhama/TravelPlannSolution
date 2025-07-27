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
    
    // Commandes de navigation manquantes
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }
    public ICommand ResetViewCommand { get; }
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
        ToggleViewModeCommand = new Command(() => ExecuteToggleViewModeCommand());
        ToggleMapStyleCommand = new Command(() => ExecuteToggleMapStyleCommand());
        GoToMyLocationCommand = new Command(async () => await ExecuteGoToMyLocationCommand());
        ToggleFiltersCommand = new Command(() => ExecuteToggleFiltersCommand());
        CloseLocationInfoCommand = new Command(() => ExecuteCloseLocationInfoCommand());
        ShowAccommodationsCommand = new Command(() => ExecuteShowAccommodationsCommand());
        ShowActivitiesCommand = new Command(() => ExecuteShowActivitiesCommand());
        ShowRestaurantsCommand = new Command(() => ExecuteShowRestaurantsCommand());
        ShowDirectionsCommand = new Command(() => ExecuteShowDirectionsCommand());
        
        // Commandes de navigation
        MoveUpCommand = new Command(() => ExecuteMoveCommand("up"));
        MoveDownCommand = new Command(() => ExecuteMoveCommand("down"));
        MoveLeftCommand = new Command(() => ExecuteMoveCommand("left"));
        MoveRightCommand = new Command(() => ExecuteMoveCommand("right"));
        ResetViewCommand = new Command(() => ExecuteResetViewCommand());
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
            UpdateMapPins();
            
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
                _mapControl.MoveToRegion(mapSpan);
                
                // Supprimer les anciens pins de recherche
                var existingSearchPins = _mapControl.Pins.Where(p => p.Type == PinType.SearchResult).ToList();
                foreach (var pin in existingSearchPins)
                {
                    _mapControl.Pins.Remove(pin);
                }
                
                // Ajouter un nouveau pin de recherche
                var searchPin = new Pin
                {
                    Location = searchResult,
                    Label = SearchQuery,
                    Address = await GetAddressFromLocationAsync(searchResult),
                    Type = PinType.SearchResult
                };
                
                _mapControl.Pins.Add(searchPin);
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

    private void ExecuteToggleViewModeCommand()
    {
        if (!_isMapInitialized) return;

        try
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de mode vue: {ex.Message}");
        }
    }

    private void ExecuteToggleMapStyleCommand()
    {
        try
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
            var location = await
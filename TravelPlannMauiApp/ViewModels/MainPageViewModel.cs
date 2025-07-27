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
    
    // Nouvelles commandes pour la navigation
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetViewCommand { get; }

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

        // Commandes existantes
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
        
        // Nouvelles commandes de navigation
        MoveUpCommand = new Command(() => ExecuteMoveCommand("up"));
        MoveDownCommand = new Command(() => ExecuteMoveCommand("down"));
        MoveLeftCommand = new Command(() => ExecuteMoveCommand("left"));
        MoveRightCommand = new Command(() => ExecuteMoveCommand("right"));
        ZoomInCommand = new Command(() => ExecuteZoomCommand(true));
        ZoomOutCommand = new Command(() => ExecuteZoomCommand(false));
        ResetViewCommand = new Command(() => ExecuteResetViewCommand());
        
        _ = LoadUserDataAsync();
    }

    public void SetMapControl(Microsoft.Maui.Controls.Maps.Map mapControl)
    {
        _mapControl = mapControl;
        
        // Configurer les événements de la carte
        if (_mapControl != null)
        {
            // CORRECTION: Configuration appropriée des interactions
            _mapControl.IsZoomEnabled = true;
            _mapControl.IsScrollEnabled = true;
            _mapControl.InputTransparent = false;
            
            // Configuration des interactions utilisateur
            ConfigureMapInteractions();
            
            // S'abonner aux événements nécessaires
            SetupMapEvents();
        }
        
        UpdateMapPins();
    }
    
    private void ConfigureMapInteractions()
    {
        if (_mapControl != null)
        {
            // Activer tous les types d'interaction
            _mapControl.IsZoomEnabled = true;
            _mapControl.IsScrollEnabled = true;
            
            // S'assurer que la carte peut recevoir les événements
            _mapControl.InputTransparent = false;
            _mapControl.CascadeInputTransparent = false;
            
            // Configuration des gestes sur les plateformes spécifiques
            try
            {
                // Ces propriétés peuvent ne pas être disponibles sur toutes les plateformes
                var mapType = _mapControl.GetType();
                var hasZoomEnabledProperty = mapType.GetProperty("HasZoomEnabled");
                var hasScrollEnabledProperty = mapType.GetProperty("HasScrollEnabled");
                
                hasZoomEnabledProperty?.SetValue(_mapControl, true);
                hasScrollEnabledProperty?.SetValue(_mapControl, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Propriétés d'interaction non disponibles: {ex.Message}");
            }
            
            System.Diagnostics.Debug.WriteLine("Interactions de la carte configurées");
        }
    }
    
    private void SetupMapEvents()
    {
        if (_mapControl != null)
        {
            _mapControl.PropertyChanged += OnMapPropertyChanged;
            
            // Gestion des clics sur les pins
            _mapControl.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_mapControl.Pins))
                {
                    foreach (var pin in _mapControl.Pins)
                    {
                        // Configuration des événements de pins
                        System.Diagnostics.Debug.WriteLine($"Pin configurée: {pin.Label}");
                    }
                }
            };
            
            System.Diagnostics.Debug.WriteLine("Événements de la carte configurés");
        }
    }
    
    private void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Microsoft.Maui.Controls.Maps.Map.VisibleRegion))
        {
            System.Diagnostics.Debug.WriteLine("Région de la carte mise à jour");
        }
    }

    #region Navigation Commands Implementation

    private void ExecuteMoveCommand(string direction)
    {
        if (_mapControl?.VisibleRegion == null) return;

        try
        {
            var currentRegion = _mapControl.VisibleRegion;
            var currentCenter = currentRegion.Center;
            var moveDistance = currentRegion.Radius.Meters * 0.3; // Déplacement de 30% du rayon visible
            
            // Calculer les offsets en degrés (approximation)
            var latOffset = moveDistance / 111320.0; // 1 degré ≈ 111.32 km
            var lonOffset = moveDistance / (111320.0 * Math.Cos(currentCenter.Latitude * Math.PI / 180.0));
            
            Location newCenter = direction switch
            {
                "up" => new Location(currentCenter.Latitude + latOffset, currentCenter.Longitude),
                "down" => new Location(currentCenter.Latitude - latOffset, currentCenter.Longitude),
                "left" => new Location(currentCenter.Latitude, currentCenter.Longitude - lonOffset),
                "right" => new Location(currentCenter.Latitude, currentCenter.Longitude + lonOffset),
                _ => currentCenter
            };
            
            var newSpan = MapSpan.FromCenterAndRadius(newCenter, currentRegion.Radius);
            _mapControl.MoveToRegion(newSpan);
            
            System.Diagnostics.Debug.WriteLine($"Déplacement {direction} vers: {newCenter.Latitude:F4}, {newCenter.Longitude:F4}");
            ShowTemporaryMessage($"Carte déplacée vers le {GetDirectionText(direction)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du déplacement {direction}: {ex.Message}");
            ShowTemporaryMessage("Erreur lors du déplacement");
        }
    }
    
    private string GetDirectionText(string direction)
    {
        return direction switch
        {
            "up" => "nord",
            "down" => "sud",
            "left" => "ouest",
            "right" => "est",
            _ => "centre"
        };
    }
    
    private void ExecuteZoomCommand(bool zoomIn)
    {
        if (_mapControl?.VisibleRegion == null) return;

        try
        {
            var currentRegion = _mapControl.VisibleRegion;
            var center = currentRegion.Center;
            var currentRadius = currentRegion.Radius.Meters;
            
            // Facteur de zoom
            var zoomFactor = zoomIn ? 0.6 :
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Authentication.WebAuthenticator;

namespace TravelPlannMauiApp.ViewModels;

public class MapViewModel : INotifyPropertyChanged
{
    private Map _mapControl;
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

    public MapViewModel()
    {
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
        
        LoadSampleData();
    }

    public void SetMapControl(Map mapControl)
    {
        _mapControl = mapControl;
        UpdateMapPins();
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
            
            // Exemple de résultat de recherche (à remplacer par une vraie API)
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
        ShowTemporaryMessage("Hébergements affichés");
    }

    private void ExecuteShowActivitiesCommand()
    {
        ShowLocationInfo = false
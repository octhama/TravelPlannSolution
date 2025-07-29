using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
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

    private bool _showPOIManagement = false;
    private bool _poiTabAccommodations = true;
    private bool _poiTabActivities = false;
    private string _poiManagementMessage = "";
    private ObservableCollection<Hebergement> _accommodationsList = new();
    private ObservableCollection<Activite> _activitiesList = new();
    
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

    public bool ShowPOIManagement
{
    get => _showPOIManagement;
    set
    {
        _showPOIManagement = value;
        OnPropertyChanged();
    }
}

public bool POITabAccommodations
{
    get => _poiTabAccommodations;
    set
    {
        _poiTabAccommodations = value;
        OnPropertyChanged();
    }
}

public bool POITabActivities
{
    get => _poiTabActivities;
    set
    {
        _poiTabActivities = value;
        OnPropertyChanged();
    }
}

public string POIManagementMessage
{
    get => _poiManagementMessage;
    set
    {
        _poiManagementMessage = value;
        OnPropertyChanged();
    }
}

public ObservableCollection<Hebergement> AccommodationsList
{
    get => _accommodationsList;
    set
    {
        _accommodationsList = value;
        OnPropertyChanged();
    }
}

public ObservableCollection<Activite> ActivitiesList
{
    get => _activitiesList;
    set
    {
        _activitiesList = value;
        OnPropertyChanged();
    }
}

public int AccommodationsCount => AccommodationsList?.Count ?? 0;
public int ActivitiesCount => ActivitiesList?.Count ?? 0;

    // Coordonnées par défaut pour Namur, Belgique
    private static readonly Location DefaultLocation = new Location(50.4674, 4.8719); // Namur
    private static readonly MapSpan DefaultRegion = MapSpan.FromCenterAndRadius(DefaultLocation, Distance.FromKilometers(10));

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
    
    public ICommand TogglePOIManagementCommand { get; }
    public ICommand ClosePOIManagementCommand { get; }
    public ICommand SwitchPOITabCommand { get; }
    public ICommand DeleteAccommodationCommand { get; }
    public ICommand DeleteActivityCommand { get; }

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

        // Initialiser les commandes (code existant)
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
        ZoomInCommand = new Command(() => ExecuteZoomInCommand());
        ZoomOutCommand = new Command(() => ExecuteZoomOutCommand());

        TogglePOIManagementCommand = new Command(() => ExecuteTogglePOIManagementCommand());
        ClosePOIManagementCommand = new Command(() => ExecuteClosePOIManagementCommand());
        SwitchPOITabCommand = new Command<string>((tab) => ExecuteSwitchPOITabCommand(tab));
        DeleteAccommodationCommand = new Command<Hebergement>(async (item) => await ExecuteDeleteAccommodationCommand(item));
        DeleteActivityCommand = new Command<Activite>(async (item) => await ExecuteDeleteActivityCommand(item));

        // Initialiser le cancellation token pour les messages
        _messagesCancellationTokenSource = new CancellationTokenSource();

        // Définir la région par défaut sur Namur
        _currentRegion = DefaultRegion;

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
            
            // NOUVEAU: Centrer immédiatement sur Namur
            _mapControl.MoveToRegion(DefaultRegion);
            _currentRegion = DefaultRegion;
            
            // Configurer les événements
            SetupMapEvents();
            
            // Mettre à jour les pins avec les données déjà chargées
            _ = Task.Run(UpdateMapPins);
            
            System.Diagnostics.Debug.WriteLine("Contrôle de carte configuré avec succès - Centré sur Namur");
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
    if (string.IsNullOrWhiteSpace(SearchQuery))
    {
        await ShowTemporaryMessageAsync("Saisissez un lieu à rechercher");
        return;
    }

    if (!_isMapInitialized)
    {
        await ShowTemporaryMessageAsync("Carte non initialisée");
        return;
    }

    IsLoading = true;
    try
    {
        System.Diagnostics.Debug.WriteLine($"Recherche en cours: {SearchQuery}");
        
        var searchResult = await GeocodeLocationAsync(SearchQuery);
        
        if (searchResult != null)
        {
            System.Diagnostics.Debug.WriteLine($"Résultat trouvé: {searchResult.Latitude:F4}, {searchResult.Longitude:F4}");
            
            // Centrer la carte sur le résultat
            var mapSpan = MapSpan.FromCenterAndRadius(searchResult, Distance.FromKilometers(2));
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _mapControl?.MoveToRegion(mapSpan);
            });
            
            // Supprimer les anciens pins de recherche
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_mapControl?.Pins != null)
                {
                    var existingSearchPins = _mapControl.Pins.Where(p => p.Type == PinType.SearchResult).ToList();
                    foreach (var pin in existingSearchPins)
                    {
                        _mapControl.Pins.Remove(pin);
                    }
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
                _mapControl?.Pins.Add(searchPin);
            });
            
            ShowLocationDetails(searchPin);
            await ShowTemporaryMessageAsync($"Lieu trouvé: {SearchQuery}");
            
            // Effacer la recherche
            SearchQuery = "";
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Aucun résultat pour: {SearchQuery}");
            await ShowTemporaryMessageAsync("Aucun résultat trouvé pour cette recherche");
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Erreur de recherche: {ex.Message}");
        await ShowTemporaryMessageAsync("Erreur lors de la recherche");
    }
    finally
    {
        IsLoading = false;
    }
}

    private async Task ExecuteToggleViewModeCommand()
{
    if (!_isMapInitialized)
    {
        await ShowTemporaryMessageAsync("Carte non initialisée");
        return;
    }

    try
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (_mapControl != null)
            {
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
                System.Diagnostics.Debug.WriteLine($"Mode carte changé vers: {_mapControl.MapType}");
            }
        });
        
        await ShowTemporaryMessageAsync($"Mode: {ViewModeIcon}");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de mode vue: {ex.Message}");
        await ShowTemporaryMessageAsync("Erreur lors du changement de vue");
    }
}

    private async Task ExecuteToggleMapStyleCommand()
{
    try
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Cette fonctionnalité dépendrait du thème de l'application
            if (MapStyleIcon == "🌙")
            {
                MapStyleIcon = "☀️";
            }
            else
            {
                MapStyleIcon = "🌙";
            }
        });
        
        await ShowTemporaryMessageAsync($"Style: {MapStyleIcon}");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de style: {ex.Message}");
        await ShowTemporaryMessageAsync("Erreur lors du changement de style");
    }
}

    private async Task ExecuteGoToMyLocationCommand()
    {
        if (!_isMapInitialized)
        {
            await ShowTemporaryMessageAsync("Carte non initialisée");
            return;
        }

        try
        {
            // Au lieu de la géolocalisation, centrer sur Namur
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _mapControl?.MoveToRegion(DefaultRegion);
                _currentRegion = DefaultRegion;
            });
            
            await ShowTemporaryMessageAsync("Centré sur Namur, Belgique");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du centrage sur Namur: {ex.Message}");
            await ShowTemporaryMessageAsync("Erreur lors du centrage");
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

    public void DiagnoseVoyageData()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== DIAGNOSTIC DONNÉES VOYAGE ===");
            System.Diagnostics.Debug.WriteLine($"Nombre total de voyages utilisateur: {_userVoyages?.Count ?? 0}");
            
            foreach (var voyage in _userVoyages ?? new List<Voyage>())
            {
                System.Diagnostics.Debug.WriteLine($"Voyage: {voyage.NomVoyage} (ID: {voyage.VoyageId})");
                System.Diagnostics.Debug.WriteLine($"  - Activités: {voyage.Activites?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  - Hébergements: {voyage.Hebergements?.Count ?? 0}");
                
                if (voyage.Activites != null)
                {
                    foreach (var activite in voyage.Activites)
                    {
                        System.Diagnostics.Debug.WriteLine($"    * Activité: {activite.Nom} - {activite.Localisation}");
                    }
                }
                
                if (voyage.Hebergements != null)
                {
                    foreach (var hebergement in voyage.Hebergements)
                    {
                        System.Diagnostics.Debug.WriteLine($"    * Hébergement: {hebergement.Nom} - {hebergement.Adresse}");
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Pins hébergements: {_accommodationPins?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"Pins activités: {_activityPins?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine("=== FIN DIAGNOSTIC ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur diagnostic: {ex.Message}");
        }
    }
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

                // DIAGNOSTIC
                DiagnoseVoyageData();

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
            System.Diagnostics.Debug.WriteLine("Début du chargement des points d'intérêt...");
            
            _accommodationPins.Clear();
            _activityPins.Clear();
            _restaurantPins.Clear();
            _transportPins.Clear();

            // Charger TOUS les hébergements
            try
            {
                var hebergements = await _hebergementService.GetAllHebergementsAsync();
                if (hebergements != null)
                {
                    foreach (var hebergement in hebergements)
                    {
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
                                System.Diagnostics.Debug.WriteLine($"Pin hébergement ajouté: {hebergement.Nom}");
                            }
                            
                            await Task.Delay(200); // Éviter de surcharger l'API de géocodage
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des hébergements: {ex.Message}");
            }

            // Charger TOUTES les activités
            try
            {
                var activites = await _activiteService.GetAllActivitesAsync();
                if (activites != null)
                {
                    foreach (var activite in activites)
                    {
                        if (!string.IsNullOrEmpty(activite.Localisation))
                        {
                            System.Diagnostics.Debug.WriteLine($"Géocodage activité: {activite.Nom} - {activite.Localisation}");
                            
                            var location = await GeocodeLocationAsync(activite.Localisation);
                            if (location != null)
                            {
                                var pin = new Pin
                                {
                                    Location = location,
                                    Label = $"🎉 {activite.Nom}",
                                    Address = activite.Localisation,
                                    Type = PinType.Place
                                };
                                _activityPins.Add(pin);
                                System.Diagnostics.Debug.WriteLine($"Pin activité ajouté: {activite.Nom}");
                            }

                            await Task.Delay(200);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des activités: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"Chargement terminé - Hébergements: {_accommodationPins.Count}, Activités: {_activityPins.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des points d'intérêt: {ex.Message}");
        }
        finally
        {
            // Mettre à jour les pins sur la carte
            await UpdateMapPins();
            System.Diagnostics.Debug.WriteLine("Chargement des points d'intérêt terminé");
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

    public async Task InitializeMapOnNamurAsync()
    {
        try
        {
            if (!_isMapInitialized)
            {
                System.Diagnostics.Debug.WriteLine("Carte non initialisée pour le centrage sur Namur");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Centrer sur Namur avec un zoom approprié
                _mapControl?.MoveToRegion(DefaultRegion);
                _currentRegion = DefaultRegion;
                
                System.Diagnostics.Debug.WriteLine($"Carte centrée sur Namur: {DefaultLocation.Latitude:F4}, {DefaultLocation.Longitude:F4}");
            });
            
            // Optionnel: Ajouter un pin pour marquer Namur
            var namurPin = new Pin
            {
                Location = DefaultLocation,
                Label = "📍 Namur",
                Address = "Namur, Belgique",
                Type = PinType.Generic
            };
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _mapControl?.Pins.Add(namurPin);
            });
            
            await ShowTemporaryMessageAsync("Bienvenue à Namur!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation sur Namur: {ex.Message}");
        }
    }
    private void ShowTemporaryMessage(string message, int durationMs = 3000)
    {
        _ = ShowTemporaryMessageAsync(message, durationMs);
    }

    private async Task UpdateMapPins()
    {
        if (!_isMapInitialized || _mapControl == null)
        {
            System.Diagnostics.Debug.WriteLine("Carte non initialisée, impossible de mettre à jour les pins");
            return;
        }

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                System.Diagnostics.Debug.WriteLine("Début de la mise à jour des pins...");

                // Supprimer tous les pins existants sauf les pins de recherche et utilisateur
                var pinsToRemove = _mapControl.Pins
                    .Where(p => p.Type != PinType.SearchResult && p.Type != PinType.Generic && p.Label != "Lieu sélectionné")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Suppression de {pinsToRemove.Count} pins existants");

                foreach (var pin in pinsToRemove)
                {
                    _mapControl.Pins.Remove(pin);
                }

                var pinsAdded = 0;

                // Ajouter les pins selon les filtres actifs
                if (ShowAccommodations)
                {
                    foreach (var pin in _accommodationPins)
                    {
                        _mapControl.Pins.Add(pin);
                        pinsAdded++;
                    }
                    System.Diagnostics.Debug.WriteLine($"Ajouté {_accommodationPins.Count} pins d'hébergements");
                }

                if (ShowActivities)
                {
                    foreach (var pin in _activityPins)
                    {
                        _mapControl.Pins.Add(pin);
                        pinsAdded++;
                    }
                    System.Diagnostics.Debug.WriteLine($"Ajouté {_activityPins.Count} pins d'activités");
                }

                if (ShowRestaurants)
                {
                    foreach (var pin in _restaurantPins)
                    {
                        _mapControl.Pins.Add(pin);
                        pinsAdded++;
                    }
                    System.Diagnostics.Debug.WriteLine($"Ajouté {_restaurantPins.Count} pins de restaurants");
                }

                if (ShowTransport)
                {
                    foreach (var pin in _transportPins)
                    {
                        _mapControl.Pins.Add(pin);
                        pinsAdded++;
                    }
                    System.Diagnostics.Debug.WriteLine($"Ajouté {_transportPins.Count} pins de transport");
                }

                var totalPins = _mapControl.Pins.Count;
                System.Diagnostics.Debug.WriteLine($"Mise à jour des pins terminée - {pinsAdded} pins ajoutés, {totalPins} total visible");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des pins: {ex.Message}");
        }
    }

    #endregion

    
    #region POI Management Methods

    private void ExecuteTogglePOIManagementCommand()
    {
        ShowPOIManagement = !ShowPOIManagement;
        if (ShowPOIManagement)
        {
            // Fermer les autres panels
            ShowFilters = false;
            ShowLocationInfo = false;
            
            // Charger les données
            _ = Task.Run(LoadPOIDataAsync);
            ShowTemporaryMessage("Gestion des lieux");
        }
    }

    private void ExecuteClosePOIManagementCommand()
    {
        ShowPOIManagement = false;
        POIManagementMessage = "";
    }

    private void ExecuteSwitchPOITabCommand(string tab)
    {
        switch (tab?.ToLower())
        {
            case "accommodations":
                POITabAccommodations = true;
                POITabActivities = false;
                break;
            case "activities":
                POITabAccommodations = false;
                POITabActivities = true;
                break;
        }
    }

    private async Task ExecuteDeleteAccommodationCommand(Hebergement hebergement)
    {
        if (hebergement == null) return;

        try
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Supprimer", 
                $"Êtes-vous sûr de vouloir supprimer l'hébergement '{hebergement.Nom}' ?", 
                "Oui", "Non");

            if (confirm)
            {
                POIManagementMessage = "Suppression en cours...";
                
                await _hebergementService.DeleteHebergementAsync(hebergement.HebergementId);
                
                // Mettre à jour les listes
                AccommodationsList.Remove(hebergement);
                OnPropertyChanged(nameof(AccommodationsCount));
                
                // Supprimer le pin correspondant
                RemoveAccommodationPin(hebergement);
                
                // Recharger les pins de la carte
                await UpdateMapPins();
                
                await ShowTemporaryMessageAsync($"Hébergement '{hebergement.Nom}' supprimé");
                POIManagementMessage = "";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression de l'hébergement: {ex.Message}");
            await ShowTemporaryMessageAsync("Erreur lors de la suppression");
            POIManagementMessage = "Erreur lors de la suppression";
        }
    }

    private async Task ExecuteDeleteActivityCommand(Activite activite)
    {
        if (activite == null) return;

        try
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Supprimer", 
                $"Êtes-vous sûr de vouloir supprimer l'activité '{activite.Nom}' ?", 
                "Oui", "Non");

            if (confirm)
            {
                POIManagementMessage = "Suppression en cours...";
                
                await _activiteService.DeleteActiviteAsync(activite.ActiviteId);
                
                // Mettre à jour les listes
                ActivitiesList.Remove(activite);
                OnPropertyChanged(nameof(ActivitiesCount));
                
                // Supprimer le pin correspondant
                RemoveActivityPin(activite);
                
                // Recharger les pins de la carte
                await UpdateMapPins();
                
                await ShowTemporaryMessageAsync($"Activité '{activite.Nom}' supprimée");
                POIManagementMessage = "";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression de l'activité: {ex.Message}");
            await ShowTemporaryMessageAsync("Erreur lors de la suppression");
            POIManagementMessage = "Erreur lors de la suppression";
        }
    }

    private async Task LoadPOIDataAsync()
    {
        try
        {
            POIManagementMessage = "Chargement en cours...";
            
            // Charger tous les hébergements
            var hebergements = await _hebergementService.GetAllHebergementsAsync();
            var activites = await _activiteService.GetAllActivitesAsync();
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AccommodationsList.Clear();
                ActivitiesList.Clear();
                
                if (hebergements != null)
                {
                    foreach (var h in hebergements)
                    {
                        AccommodationsList.Add(h);
                    }
                }
                
                if (activites != null)
                {
                    foreach (var a in activites)
                    {
                        ActivitiesList.Add(a);
                    }
                }
                
                OnPropertyChanged(nameof(AccommodationsCount));
                OnPropertyChanged(nameof(ActivitiesCount));
                POIManagementMessage = "";
            });
            
            System.Diagnostics.Debug.WriteLine($"Données POI chargées: {AccommodationsCount} hébergements, {ActivitiesCount} activités");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des données POI: {ex.Message}");
            POIManagementMessage = "Erreur lors du chargement";
        }
    }

    private void RemoveAccommodationPin(Hebergement hebergement)
    {
        try
        {
            var pinToRemove = _accommodationPins.FirstOrDefault(p => p.Label.Contains(hebergement.Nom));
            if (pinToRemove != null)
            {
                _accommodationPins.Remove(pinToRemove);
                
                // Supprimer également de la carte si visible
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_mapControl?.Pins.Contains(pinToRemove) == true)
                    {
                        _mapControl.Pins.Remove(pinToRemove);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression du pin hébergement: {ex.Message}");
        }
    }

    private void RemoveActivityPin(Activite activite)
    {
        try
        {
            var pinToRemove = _activityPins.FirstOrDefault(p => p.Label.Contains(activite.Nom));
            if (pinToRemove != null)
            {
                _activityPins.Remove(pinToRemove);
                
                // Supprimer également de la carte si visible
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_mapControl?.Pins.Contains(pinToRemove) == true)
                    {
                        _mapControl.Pins.Remove(pinToRemove);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression du pin activité: {ex.Message}");
        }
    }

    #endregion

    #region Location Services

    public async Task<Location> GeocodeLocationAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                System.Diagnostics.Debug.WriteLine("Adresse vide pour le géocodage");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Tentative de géocodage: '{address}'");

            // Nettoyer l'adresse
            var cleanAddress = address.Trim();

            var locations = await Geocoding.Default.GetLocationsAsync(cleanAddress);
            var location = locations?.FirstOrDefault();

            if (location != null)
            {
                System.Diagnostics.Debug.WriteLine($"Géocodage réussi pour '{cleanAddress}': {location.Latitude:F6}, {location.Longitude:F6}");
                return location;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Aucun résultat de géocodage pour '{cleanAddress}'");

                // Essayer une version simplifiée de l'adresse
                var simplifiedAddress = ExtractMainLocation(cleanAddress);
                if (!string.IsNullOrEmpty(simplifiedAddress) && simplifiedAddress != cleanAddress)
                {
                    System.Diagnostics.Debug.WriteLine($"Tentative avec adresse simplifiée: '{simplifiedAddress}'");
                    var simplifiedLocations = await Geocoding.Default.GetLocationsAsync(simplifiedAddress);
                    var simplifiedLocation = simplifiedLocations?.FirstOrDefault();

                    if (simplifiedLocation != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Géocodage réussi avec adresse simplifiée: {simplifiedLocation.Latitude:F6}, {simplifiedLocation.Longitude:F6}");
                        return simplifiedLocation;
                    }
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de géocodage pour '{address}': {ex.Message}");
            return null;
        }
    }
    private string ExtractMainLocation(string fullAddress)
{
    try
    {
        if (string.IsNullOrEmpty(fullAddress))
            return null;

        // Essayer d'extraire la ville et le pays
        var parts = fullAddress.Split(',');
        if (parts.Length >= 2)
        {
            // Prendre les deux dernières parties (généralement ville, pays)
            var city = parts[parts.Length - 2].Trim();
            var country = parts[parts.Length - 1].Trim();
            return $"{city}, {country}";
        }
        
        return fullAddress;
    }
    catch
    {
        return fullAddress;
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

    // Continuation de ShowTemporaryMessage
       private async Task ShowTemporaryMessageAsync(string message, int durationMs = 3000)
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
                
                System.Diagnostics.Debug.WriteLine($"Message affiché: {message}");
                
                // Masquer le message après le délai spécifié
                try
                {
                    await Task.Delay(durationMs, _messagesCancellationTokenSource.Token);
                    
                    if (!_messagesCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            ShowMessage = false;
                            MessageText = "";
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    // Message annulé par un nouveau message - comportement normal
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage du message: {ex.Message}");
            }
        }


    #endregion

    #region Distance and Utility Methods

    /// <summary>
    /// Calcule la distance entre deux locations
    /// </summary>
    public double CalculateDistance(Location location1, Location location2, DistanceUnits units = DistanceUnits.Kilometers)
    {
        try
        {
            if (location1 == null || location2 == null)
                return 0;

            return Location.CalculateDistance(location1, location2, units);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du calcul de distance: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Vérifie si une location est dans un rayon donné par rapport à un centre
    /// </summary>
    public bool IsLocationWithinRadius(Location center, Location target, double radiusKm)
    {
        try
        {
            if (center == null || target == null)
                return false;

            var distance = CalculateDistance(center, target, DistanceUnits.Kilometers);
            return distance <= radiusKm;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la vérification du rayon: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Formate une distance pour l'affichage
    /// </summary>
    public string FormatDistance(double distanceKm)
    {
        try
        {
            if (distanceKm < 0.1)
                return $"{distanceKm * 1000:F0}m";
            else if (distanceKm < 1)
                return $"{distanceKm * 1000:F0}m";
            else if (distanceKm < 10)
                return $"{distanceKm:F1}km";
            else
                return $"{distanceKm:F0}km";
        }
        catch
        {
            return "Distance inconnue";
        }
    }

    #endregion

    #region Pin Management Helpers

    /// <summary>
    /// Trouve un pin par son label
    /// </summary>
    public Pin FindPinByLabel(string label)
    {
        try
        {
            if (!_isMapInitialized || string.IsNullOrEmpty(label))
                return null;

            return _mapControl?.Pins.FirstOrDefault(p => p.Label == label);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la recherche de pin: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Supprime un pin spécifique
    /// </summary>
    public bool RemovePin(Pin pin)
    {
        try
        {
            if (!_isMapInitialized || pin == null)
                return false;

            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                return _mapControl?.Pins.Remove(pin) ?? false;
            }).Result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression du pin: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Supprime tous les pins de recherche
    /// </summary>
    public void ClearSearchPins()
    {
        try
        {
            if (!_isMapInitialized) return;

            MainThread.InvokeOnMainThreadAsync(() =>
            {
                var searchPins = _mapControl?.Pins
                    .Where(p => p.Type == PinType.SearchResult)
                    .ToList();

                if (searchPins != null)
                {
                    foreach (var pin in searchPins)
                    {
                        _mapControl.Pins.Remove(pin);
                    }
                    System.Diagnostics.Debug.WriteLine($"{searchPins.Count} pins de recherche supprimés");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du nettoyage des pins de recherche: {ex.Message}");
        }
    }

    /// <summary>
    /// Compte les pins par type
    /// </summary>
    public Dictionary<string, int> GetPinCounts()
    {
        try
        {
            var counts = new Dictionary<string, int>
            {
                ["Hébergements"] = _accommodationPins.Count,
                ["Activités"] = _activityPins.Count,
                ["Restaurants"] = _restaurantPins.Count,
                ["Transports"] = _transportPins.Count,
                ["Total Visible"] = _isMapInitialized ? _mapControl?.Pins.Count ?? 0 : 0
            };

            return counts;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du comptage des pins: {ex.Message}");
            return new Dictionary<string, int>();
        }
    }

    #endregion

    #region Map Navigation Helpers

    /// <summary>
    /// Centre la carte sur les pins d'un voyage spécifique
    /// </summary>
    public async Task CenterOnVoyageAsync(int voyageId)
    {
        try
        {
            if (!_isMapInitialized) return;

            var voyage = _userVoyages.FirstOrDefault(v => v.VoyageId == voyageId);
            if (voyage == null) return;

            var relevantPins = new List<Pin>();
            
            // CORRECTION: Utiliser les noms pour faire correspondre les pins (pas VoyageId pour Activite)
            if (voyage.Hebergements != null && voyage.Hebergements.Any())
            {
                relevantPins.AddRange(_accommodationPins.Where(p => 
                    voyage.Hebergements.Any(h => p.Label.Contains(h.Nom))));
            }
            
            if (voyage.Activites != null && voyage.Activites.Any())
            {
                relevantPins.AddRange(_activityPins.Where(p => 
                    voyage.Activites.Any(a => p.Label.Contains(a.Nom))));
            }

            if (relevantPins.Any())
            {
                await CenterOnPinsAsync(relevantPins);
                await ShowTemporaryMessageAsync($"Centré sur le voyage: {voyage.NomVoyage}");
            }
            else
            {
                await ShowTemporaryMessageAsync($"Aucun lieu trouvé pour le voyage: {voyage.NomVoyage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du centrage sur voyage: {ex.Message}");
        await ShowTemporaryMessageAsync("Erreur lors du centrage sur le voyage");
    }
}

    /// <summary>
    /// Centre la carte sur une liste de pins
    /// </summary>
    public async Task CenterOnPinsAsync(List<Pin> pins, double paddingKm = 2)
    {
        try
        {
            if (!_isMapInitialized || pins == null || !pins.Any()) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var minLat = pins.Min(p => p.Location.Latitude);
                var maxLat = pins.Max(p => p.Location.Latitude);
                var minLon = pins.Min(p => p.Location.Longitude);
                var maxLon = pins.Max(p => p.Location.Longitude);

                var centerLat = (minLat + maxLat) / 2;
                var centerLon = (minLon + maxLon) / 2;
                var center = new Location(centerLat, centerLon);

                // Calculer le rayon nécessaire
                var maxDistance = pins.Max(p => CalculateDistance(center, p.Location));
                var radius = Math.Max(maxDistance + paddingKm, 1); // Minimum 1km

                var region = MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(radius));
                _mapControl?.MoveToRegion(region);

                System.Diagnostics.Debug.WriteLine($"Carte centrée sur {pins.Count} pins - Rayon: {radius:F1}km");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du centrage sur pins: {ex.Message}");
        }
    }

    /// <summary>
    /// Anime vers une location avec un zoom adaptatif
    /// </summary>
    public async Task AnimateToLocationAsync(Location location, double? radiusKm = null)
    {
        try
        {
            if (!_isMapInitialized || location == null) return;

            // Déterminer le rayon selon le contexte
            var radius = radiusKm ?? (_currentRegion != null ? 
                _currentRegion.Radius.Kilometers : 2.0);

            var region = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(radius));

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _mapControl?.MoveToRegion(region);
            });

            // Simuler une animation avec un délai
            await Task.Delay(500);

            System.Diagnostics.Debug.WriteLine($"Animation vers: {location.Latitude:F4}, {location.Longitude:F4}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'animation: {ex.Message}");
        }
    }

    #endregion

    #region Export and Share Methods

    /// <summary>
    /// Exporte les informations de la carte actuelle
    /// </summary>
    public Dictionary<string, object> ExportMapState()
    {
        try
        {
            var mapState = new Dictionary<string, object>
            {
                ["CurrentRegion"] = _currentRegion != null ? new
                {
                    Center = new { _currentRegion.Center.Latitude, _currentRegion.Center.Longitude },
                    RadiusKm = _currentRegion.Radius.Kilometers
                } : null,
                ["UserLocation"] = _userLocation != null ? new
                {
                    _userLocation.Latitude,
                    _userLocation.Longitude
                } : null,
                ["FilterSettings"] = new
                {
                    ShowAccommodations,
                    ShowActivities,
                    ShowRestaurants,
                    ShowTransport
                },
                ["PinCounts"] = GetPinCounts(),
                ["MapType"] = _isMapInitialized ? _mapControl?.MapType.ToString() : "Unknown",
                ["ExportTime"] = DateTime.Now
            };

            return mapState;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'export de l'état: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    #endregion

    #region INotifyPropertyChanged Implementation

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
            return false;

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region IDisposable Implementation

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    // Nettoyer les événements
                    if (_mapControl != null)
                    {
                        _mapControl.PropertyChanged -= OnMapPropertyChanged;
                    }

                    // Annuler les tâches en cours
                    _messagesCancellationTokenSource?.Cancel();
                    _messagesCancellationTokenSource?.Dispose();

                    // Nettoyer les collections
                    _accommodationPins?.Clear();
                    _activityPins?.Clear();
                    _restaurantPins?.Clear();
                    _transportPins?.Clear();
                    _userVoyages?.Clear();

                    System.Diagnostics.Debug.WriteLine("MapViewModel dispose() terminé");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors du dispose: {ex.Message}");
                }
            }

            _disposed = true;
        }
    }

    ~MapViewModel()
    {
        Dispose(false);
    }

    #endregion

    #region Debug and Logging Methods

    /// <summary>
    /// Affiche l'état actuel du ViewModel pour debug
    /// </summary>
    public void LogCurrentState()
    {
        try
        {
            var state = new
            {
                IsMapInitialized = _isMapInitialized,
                IsLoading,
                ShowLocationInfo,
                ShowFilters,
                UserVoyagesCount = _userVoyages?.Count ?? 0,
                AccommodationPinsCount = _accommodationPins?.Count ?? 0,
                ActivityPinsCount = _activityPins?.Count ?? 0,
                CurrentRegion = _currentRegion != null ? 
                    $"{_currentRegion.Center.Latitude:F4},{_currentRegion.Center.Longitude:F4} ({_currentRegion.Radius.Kilometers:F1}km)" : 
                    "Non définie",
                UserLocation = _userLocation != null ? 
                    $"{_userLocation.Latitude:F4},{_userLocation.Longitude:F4}" : 
                    "Non définie",
                MapPinsCount = _isMapInitialized ? _mapControl?.Pins.Count ?? 0 : 0
            };

            System.Diagnostics.Debug.WriteLine($"=== État MapViewModel ===");
            System.Diagnostics.Debug.WriteLine($"Carte initialisée: {state.IsMapInitialized}");
            System.Diagnostics.Debug.WriteLine($"Chargement: {state.IsLoading}");
            System.Diagnostics.Debug.WriteLine($"Voyages utilisateur: {state.UserVoyagesCount}");
            System.Diagnostics.Debug.WriteLine($"Pins hébergements: {state.AccommodationPinsCount}");
            System.Diagnostics.Debug.WriteLine($"Pins activités: {state.ActivityPinsCount}");
            System.Diagnostics.Debug.WriteLine($"Région actuelle: {state.CurrentRegion}");
            System.Diagnostics.Debug.WriteLine($"Position utilisateur: {state.UserLocation}");
            System.Diagnostics.Debug.WriteLine($"Pins sur carte: {state.MapPinsCount}");
            System.Diagnostics.Debug.WriteLine($"=========================");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du logging: {ex.Message}");
        }
    }

    #endregion
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TravelPlannMauiApp.ViewModels;

public class MapViewModel : INotifyPropertyChanged
{
    private WebView _webView;
    private string _searchQuery = "";
    private bool _isLoading = false;
    private bool _showLocationInfo = false;
    private string _selectedLocationName = "";
    private string _selectedLocationAddress = "";
    private string _viewModeIcon = "🗺️";

    public event PropertyChangedEventHandler PropertyChanged;

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

    public string ViewModeIcon
    {
        get => _viewModeIcon;
        set
        {
            _viewModeIcon = value;
            OnPropertyChanged();
        }
    }

    public ICommand SearchCommand { get; }
    public ICommand ToggleViewModeCommand { get; }
    public ICommand GoToMyLocationCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ShowAccommodationsCommand { get; }
    public ICommand ShowActivitiesCommand { get; }

    public MapViewModel()
    {
        SearchCommand = new Command(async () => await ExecuteSearchCommand());
        ToggleViewModeCommand = new Command(async () => await ExecuteToggleViewModeCommand());
        GoToMyLocationCommand = new Command(async () => await ExecuteGoToMyLocationCommand());
        ZoomInCommand = new Command(async () => await ExecuteZoomInCommand());
        ZoomOutCommand = new Command(async () => await ExecuteZoomOutCommand());
        ShowAccommodationsCommand = new Command(() => ExecuteShowAccommodationsCommand());
        ShowActivitiesCommand = new Command(() => ExecuteShowActivitiesCommand());
    }

    public void SetWebView(WebView webView)
    {
        _webView = webView;
    }

    private async Task ExecuteSearchCommand()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || _webView == null)
            return;

        IsLoading = true;
        try
        {
            await _webView.EvaluateJavaScriptAsync($"searchLocationFromApp('{SearchQuery}')");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de recherche: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteToggleViewModeCommand()
    {
        if (_webView == null) return;

        try
        {
            var result = await _webView.EvaluateJavaScriptAsync("toggleViewModeFromApp()");
            ViewModeIcon = result.Contains("🌍") ? "🌍" : "🗺️";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de changement de vue: {ex.Message}");
        }
    }

    private async Task ExecuteGoToMyLocationCommand()
    {
        if (_webView == null) return;

        IsLoading = true;
        try
        {
            await _webView.EvaluateJavaScriptAsync("goToMyLocationFromApp()");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de géolocalisation: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteZoomInCommand()
    {
        if (_webView == null) return;

        try
        {
            await _webView.EvaluateJavaScriptAsync("zoomInFromApp()");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de zoom: {ex.Message}");
        }
    }

    private async Task ExecuteZoomOutCommand()
    {
        if (_webView == null) return;

        try
        {
            await _webView.EvaluateJavaScriptAsync("zoomOutFromApp()");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de zoom: {ex.Message}");
        }
    }

    private void ExecuteShowAccommodationsCommand()
    {
        ShowLocationInfo = false;
        // Ici, vous pourriez naviguer vers une page d'hébergements
        // ou ouvrir une modal avec les hébergements de la zone
        Application.Current?.MainPage?.DisplayAlert("Hébergements", 
            $"Recherche d'hébergements pour {SelectedLocationName}", "OK");
    }

    private void ExecuteShowActivitiesCommand()
    {
        ShowLocationInfo = false;
        // Ici, vous pourriez naviguer vers une page d'activités
        // ou ouvrir une modal avec les activités de la zone
        Application.Current?.MainPage?.DisplayAlert("Activités", 
            $"Recherche d'activités pour {SelectedLocationName}", "OK");
    }

    public void OnLocationSelected(string name, string address)
    {
        SelectedLocationName = name;
        SelectedLocationAddress = address;
        ShowLocationInfo = true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;
using BU.Services;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly IVoyageService _voyageService;
    private readonly ISessionService _sessionService;
    private int _totalVoyages;
    private int _totalPays;
    private decimal _totalDepenses;
    private string _destinationRecommande;
    private string _meteoDescription;
    private string _temperature;

    public int TotalVoyages
    {
        get => _totalVoyages;
        set { _totalVoyages = value; OnPropertyChanged(); }
    }

    public int TotalPays
    {
        get => _totalPays;
        set { _totalPays = value; OnPropertyChanged(); }
    }

    public decimal TotalDepenses
    {
        get => _totalDepenses;
        set { _totalDepenses = value; OnPropertyChanged(); }
    }

    public string DestinationRecommande
    {
        get => _destinationRecommande;
        set { _destinationRecommande = value; OnPropertyChanged(); }
    }

    public string MeteoDescription
    {
        get => _meteoDescription;
        set { _meteoDescription = value; OnPropertyChanged(); }
    }

    public string Temperature
    {
        get => _temperature;
        set { _temperature = value; OnPropertyChanged(); }
    }

    public MainPageViewModel(IVoyageService voyageService, ISessionService sessionService)
    {
        _voyageService = voyageService;
        _sessionService = sessionService;
        _ = LoadDataAsync();
    }

    public async Task LoadUserInfoAsync()
        {
            try
            {
                var userId = await _sessionService.GetCurrentUserIdAsync();
                if (userId.HasValue)
                {
                    // Add logic to load user-specific information here
                    Debug.WriteLine($"User ID: {userId.Value}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur chargement utilisateur: {ex}");
            }
        }
    public async Task LoadDataAsync()
    {
        await LoadUserInfoAsync();
        await LoadVoyageStatsAsync();
        await LoadDestinationRecommandeAsync();
        await LoadMeteoAsync();
    }

    private async Task LoadVoyageStatsAsync()
    {
        try
        {
            var userId = await _sessionService.GetCurrentUserIdAsync();
            if (userId.HasValue)
            {
                var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(userId.Value);
                TotalVoyages = voyages.Count;
                TotalPays = voyages.Select(v => v.NomVoyage.Split(',').Last().Trim()).Distinct().Count();
                TotalDepenses = voyages.Sum(v => v.Hebergements?.Sum(h => h.Cout) ?? 0);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur stats voyages: {ex}");
        }
    }

    private async Task LoadDestinationRecommandeAsync()
    {
        try
        {
            var userId = await _sessionService.GetCurrentUserIdAsync();
            if (userId.HasValue)
            {
                var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(userId.Value);
                var voyageRecommande = voyages.FirstOrDefault(v => !v.EstArchive);
                DestinationRecommande = voyageRecommande?.NomVoyage ?? "Aucune destination recommandée";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur recommandation: {ex}");
            DestinationRecommande = "Erreur de chargement";
        }
    }

    private async Task LoadMeteoAsync()
    {
        try
        {
            var apiKey = "VOTRE_CLE_API";
            var ville = DestinationRecommande.Split(',').First().Trim();
            var api = RestService.For<IOpenWeatherApi>("https://api.openweathermap.org");
            
            // Configuration pour System.Text.Json dans Refit
            var meteo = await api.GetMeteoAsync(ville, apiKey, "fr", "metric");

            Temperature = $"{meteo.Main.Temp}°C";
            MeteoDescription = meteo.Weather.FirstOrDefault()?.Description ?? "Inconnu";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur météo: {ex}");
            Temperature = "N/A";
            MeteoDescription = "Données indisponibles";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Configuration Refit avec System.Text.Json
[Headers("Content-Type: application/json")]
public interface IOpenWeatherApi
{
    [Get("/data/2.5/weather?q={ville}&appid={apiKey}&lang={lang}&units={units}")]
    Task<OpenWeatherResponse> GetMeteoAsync(string ville, string apiKey, string lang, string units);
}

// Classes de réponse avec attributs System.Text.Json
public class OpenWeatherResponse
{
    [JsonPropertyName("main")]
    public MainInfo Main { get; set; } = default!;

    [JsonPropertyName("weather")]
    public List<WeatherInfo> Weather { get; set; } = new();
}

public class MainInfo
{
    [JsonPropertyName("temp")]
    public decimal Temp { get; set; }
}

public class WeatherInfo
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
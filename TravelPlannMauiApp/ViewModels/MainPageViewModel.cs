using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;
using BU.Services;

namespace TravelPlannMauiApp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly IVoyageService? _voyageService;
        private readonly ISessionService? _sessionService;
        private int _totalVoyages;
        private int _totalPays;
        private decimal _totalDepenses;
        private string _destinationRecommande = "Destination recommandée";
        private string _meteoDescription = "Ensoleillé";
        private string _temperature = "22°C";
        private string _userName = "Utilisateur";
        private string _meteoIcon = "☀️";

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

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        public string MeteoIcon
        {
            get => _meteoIcon;
            set { _meteoIcon = value; OnPropertyChanged(); }
        }

        public MainPageViewModel(IVoyageService? voyageService, ISessionService? sessionService)
        {
            _voyageService = voyageService;
            _sessionService = sessionService;
            
            // Initialiser avec des valeurs par défaut
            InitializeDefaultValues();
            
            // Charger les données asynchrones
            _ = LoadDataAsync();
        }

        private void InitializeDefaultValues()
        {
            TotalVoyages = 0;
            TotalPays = 0;
            TotalDepenses = 0;
            DestinationRecommande = "Paris, France";
            MeteoDescription = "Ensoleillé";
            Temperature = "22°C";
            UserName = "Voyageur";
            MeteoIcon = "☀️";
        }

        public async Task LoadUserInfoAsync()
        {
            try
            {
                if (_sessionService != null)
                {
                    var userId = await _sessionService.GetCurrentUserIdAsync();
                    var userName = await _sessionService.GetCurrentUserNameAsync();
                    
                    if (userId.HasValue)
                    {
                        UserName = userName ?? "Voyageur";
                        Debug.WriteLine($"User ID: {userId.Value}, Name: {UserName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur chargement utilisateur: {ex}");
                UserName = "Voyageur";
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
                if (_sessionService != null && _voyageService != null)
                {
                    var userId = await _sessionService.GetCurrentUserIdAsync();
                    if (userId.HasValue)
                    {
                        var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(userId.Value);
                        TotalVoyages = voyages.Count;
                        TotalPays = voyages.Select(v => v.NomVoyage.Split(',').LastOrDefault()?.Trim() ?? "").Where(p => !string.IsNullOrEmpty(p)).Distinct().Count();
                        TotalDepenses = voyages.Sum(v => v.Hebergements?.Sum(h => h.Cout) ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur stats voyages: {ex}");
                // Garder les valeurs par défaut en cas d'erreur
            }
        }

        private async Task LoadDestinationRecommandeAsync()
        {
            try
            {
                if (_sessionService != null && _voyageService != null)
                {
                    var userId = await _sessionService.GetCurrentUserIdAsync();
                    if (userId.HasValue)
                    {
                        var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(userId.Value);
                        var voyageRecommande = voyages.FirstOrDefault(v => !v.EstArchive);
                        DestinationRecommande = voyageRecommande?.NomVoyage ?? "Paris, France";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur recommandation: {ex}");
                DestinationRecommande = "Paris, France";
            }
        }

        private async Task LoadMeteoAsync()
        {
            try
            {
                // Pour l'instant, utiliser des données mockées
                // TODO: Implémenter l'appel API météo réel
                var random = new Random();
                var temperatures = new[] { "18°C", "22°C", "25°C", "19°C", "23°C" };
                var descriptions = new[] { "Ensoleillé", "Nuageux", "Partiellement nuageux", "Pluvieux" };
                var icons = new[] { "☀️", "☁️", "⛅", "🌧️" };
                
                var index = random.Next(descriptions.Length);
                Temperature = temperatures[random.Next(temperatures.Length)];
                MeteoDescription = descriptions[index];
                MeteoIcon = icons[index];
                
                await Task.Delay(100); // Simuler un appel réseau
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur météo: {ex}");
                Temperature = "22°C";
                MeteoDescription = "Données indisponibles";
                MeteoIcon = "☀️";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Interface pour l'API météo (à implémenter plus tard)
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
        public MainInfo Main { get; set; } = new();

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
}
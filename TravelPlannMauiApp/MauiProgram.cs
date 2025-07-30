using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using BU.Services;
using DAL.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using TravelPlannMauiApp.Pages;
using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps() // Ajoutez cette ligne
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => 
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuration pour lire le fichier appsettings.json depuis les ressources embarquées
        var assembly = Assembly.GetExecutingAssembly(); // Récupère l'assembly courant
        using var stream = assembly.GetManifestResourceStream("TravelPlannMauiApp.appsettings.json");
        
        if (stream == null)
        {
            throw new FileNotFoundException("Le fichier appsettings.json n'a pas été trouvé dans les ressources embarquées.");
        }

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        // Configuration DB avec la chaîne depuis appsettings.json
        var connectionString = config.GetConnectionString("TravelPlannConnectionString") ?? 
                              "Server=localhost,1433;Database=TravelPlanner;User Id=sa;Password=1235OHdf%e;TrustServerCertificate=True;";

        Debug.WriteLine($"Chaîne de connexion: {connectionString}");

        builder.Services.AddDbContextFactory<TravelPlannDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });
            
#if DEBUG
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.LogTo(message => Debug.WriteLine(message));
#endif
        });

        // Services 
        builder.Services.AddScoped<IActiviteService, ActiviteService>();
        builder.Services.AddScoped<IHebergementService, HebergementService>();
        builder.Services.AddScoped<IVoyageService, VoyageService>();
        builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ISessionService, SessionService>();

        // Enregistrement des ViewModels
        builder.Services.AddTransient<VoyageViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<VoyageDetailsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AddVoyageViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<MainPageViewModel>();

        // Configuration HttpClient pour les appels API futurs
        builder.Services.AddHttpClient("WeatherApi", client =>
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org");
            client.DefaultRequestHeaders.Add("User-Agent", "TravelPlannApp/1.0");
        });
        
        // Enregistrement des Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<VoyageListPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<VoyageDetailsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AddVoyagePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

        var app = builder.Build();
        
        // Test de la configuration au démarrage avec gestion d'erreur améliorée
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                Debug.WriteLine("Test de configuration des services...");
                
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TravelPlannDbContext>>();
                Debug.WriteLine("DbContextFactory obtenu");
                
                var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
                Debug.WriteLine("UtilisateurService obtenu");
                
                // Test de connexion DB
                using var context = dbFactory.CreateDbContext();
                var canConnect = context.Database.CanConnect();
                Debug.WriteLine($"Test de connexion DB: {canConnect}");
                
                if (!canConnect)
                {
                    Debug.WriteLine("ATTENTION: Connexion à la base de données impossible");
                }
                
                Debug.WriteLine("Services configurés correctement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERREUR de configuration des services: {ex.Message}");
                Debug.WriteLine($"Type: {ex.GetType().Name}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        return app;
    }     
}
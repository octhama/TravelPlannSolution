using System.Diagnostics;
using System.Reflection;
using BU.Services;
using DAL.DB;
using Microsoft.EntityFrameworkCore;
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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => 
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuration pour lire le fichier appsettings.json depuis les ressources embarquées
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("TravelPlannMauiApp.appsettings.json");
        
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        // Configuration DB avec la chaîne depuis appsettings.json
        var connectionString = config.GetConnectionString("TravelPlannConnectionString") ?? 
                              "Server=localhost,1433;Database=TravelPlanner;User Id=sa;Password=1235OHdf%e;TrustServerCertificate=True;";

        // Configuration DbContext avec Factory
        builder.Services.AddDbContextFactory<TravelPlannDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        // Ajouter aussi DbContext pour compatibilité
        builder.Services.AddDbContext<TravelPlannDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        // Services - IMPORTANT: S'assurer que tous les services sont enregistrés
        builder.Services.AddScoped<IActiviteService, ActiviteService>();
        builder.Services.AddScoped<IHebergementService, HebergementService>();
        builder.Services.AddScoped<IVoyageService, VoyageService>();
        builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();

        // Enregistrement des ViewModels - IMPORTANT: Tous en Transient pour éviter les problèmes de cycle de vie
        builder.Services.AddTransient<VoyageViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<VoyageDetailsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AddVoyageViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<MainPageViewModel>();
        
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
        
        // Test de la configuration au démarrage
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TravelPlannDbContext>>();
                var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
                Debug.WriteLine("Services configurés correctement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur de configuration des services: {ex}");
            }
        }

        return app;
    }     
}
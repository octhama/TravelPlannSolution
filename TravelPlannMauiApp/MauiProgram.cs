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
        builder.Services.AddDbContext<TravelPlannDbContext>(options =>
        {
            var connectionString = config.GetConnectionString("TravelPlannConnectionString");
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

        // Services
        builder.Services.AddScoped<IActiviteService, ActiviteService>();
        builder.Services.AddScoped<IHebergementService, HebergementService>();
        builder.Services.AddScoped<IVoyageService, VoyageService>();
        builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();

        // Enregistrement des ViewModels
        builder.Services.AddTransient<VoyageViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<VoyageDetailsViewModel>();

        // Enregistrement des Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<VoyageListPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<VoyageDetailsPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }     
}
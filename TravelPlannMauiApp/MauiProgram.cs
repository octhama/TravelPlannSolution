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

        // Configuration pour la console
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var options = new DbContextOptionsBuilder<TravelPlannDbContext>()
            .UseSqlServer(config.GetConnectionString("TravelPlannConnectionString"))
            .Options;

        using var context = new TravelPlannDbContext(options);

        // Configuration DB avec la chaîne depuis appsettings.json
        builder.Services.AddDbContext<TravelPlannDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("TravelPlannConnectionString");
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

        // ViewModels et Pages
        builder.Services.AddTransient<VoyageViewModel>();
        builder.Services.AddTransient<AddVoyageViewModel>();
        builder.Services.AddTransient<VoyageDetailsViewModel>();
        
        builder.Services.AddTransient<VoyageListPage>();
        builder.Services.AddTransient<VoyageDetailsPage>();
        builder.Services.AddTransient<AddVoyagePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }     
}
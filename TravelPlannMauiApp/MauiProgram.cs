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

        // Configuration DB Microsoft SQL Server
        builder.Services.AddDbContext<TravelPlannDbContext>(options =>
        {
            var connectionString = $"Server=localhost,1433;Database=TravelPlanner;User Id=sa;Password=1235OHdf%e;TrustServerCertificate=True;";
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            
            // IMPORTANT: Retirer NoTracking pour éviter les problèmes de concurrence
            // options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<AuthViewModel>();
        builder.Services.AddSingleton<MainPageViewModel>();

        builder.Services.AddTransient<ConnexionPage>();
        builder.Services.AddTransient<InscriptionPage>();

        // CORRECTION PRINCIPALE: Changer les services en Scoped au lieu de Singleton
        builder.Services.AddScoped<IActiviteService, ActiviteService>();
        builder.Services.AddScoped<IHebergementService, HebergementService>();
        builder.Services.AddScoped<IVoyageService, VoyageService>();

        // ViewModels et Pages en Transient
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
using System.Diagnostics;
using DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace TravelPlannMauiApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
            // Log the error
            if (args.ExceptionObject is Exception ex)
            {
                Console.WriteLine($"CRASH: {ex}");
            }
        };
        MainPage = new AppShell();
    }
    protected override async void OnStart()
    {
        try 
        {
            var dbFactory = Handler.MauiContext.Services
                .GetRequiredService<IDbContextFactory<TravelPlannDbContext>>();
            
            // Pré-charge les données au démarrage
            await using var context = await dbFactory.CreateDbContextAsync();
            await context.Voyages.AsNoTracking().CountAsync(); // Pré-chauffe la connexion
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DB connection failed: {ex}");
        }
    }
}
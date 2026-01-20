using System.Diagnostics;
using DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace TravelPlannMauiApp;

public partial class App : Application
{
    public App()
    {
        try
        {
            Debug.WriteLine("App: Début InitializeComponent");
            InitializeComponent();
            Debug.WriteLine("App: InitializeComponent terminé");
            
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                if (args.ExceptionObject is Exception ex)
                {
                    Debug.WriteLine($"CRASH non géré: {ex}");
                    Console.WriteLine($"CRASH: {ex}");
                }
            };
            
            Debug.WriteLine("App: Création AppShell");
            MainPage = new AppShell();
            Debug.WriteLine("App: AppShell créé avec succès");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERREUR App(): {ex}");
            Console.WriteLine($"ERREUR App(): {ex}");
            // Créer une page d'erreur basique si AppShell échoue
            MainPage = new ContentPage
            {
                Content = new Label
                {
                    Text = $"Erreur au démarrage: {ex.Message}",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };
        }
    }
    
    protected override void OnStart()
    {
        Debug.WriteLine("App: OnStart appelé");
        // Ne pas accéder à Handler ici - il peut être null
        // La connexion DB sera initialisée à la demande via DI
    }
}
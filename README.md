# Instructions d'installation - TravelPlann MAUI App

## Configuration Windows avec Visual Studio et SQL Server local

### Prérequis

* Windows 10/11
* Visual Studio 2022 (Community, Professional ou Enterprise)
* SQL Server (Express, Developer ou Standard) installé localement
* SQL Server Management Studio (SSMS)
* .NET 8 SDK ou supérieur

---

## 1. Configuration SQL Server

### A. Vérifier l'installation SQL Server

1. Ouvrez **SQL Server Configuration Manager**
   * Cherchez "SQL Server Configuration Manager" dans le menu Démarrer
   * Ou allez dans `C:\Windows\SysWOW64\SQLServerManager15.msc` (pour SQL Server 2019)
2. Dans  **SQL Server Services** , vérifiez que ces services sont démarrés :
   * `SQL Server (MSSQLSERVER)` ou `SQL Server (nom_de_votre_instance)`
   * `SQL Server Agent` (optionnel)

### B. Activer l'authentification mixte

1. Ouvrez **SQL Server Management Studio (SSMS)**
2. Connectez-vous avec l'authentification Windows
3. Clic droit sur le serveur → **Propriétés**
4. Allez dans **Sécurité** → Sélectionnez **Mode d'authentification SQL Server et Windows**
5. Cliquez **OK** et redémarrez le service SQL Server

### C. Créer la base de données et l'utilisateur

## 2. Configuration du projet MAUI

### A. Ouvrir le projet dans Visual Studio

1. Clonez le repository ou téléchargez les fichiers
2. Ouvrez **Visual Studio 2022**
3. **Fichier** → **Ouvrir** → **Projet/Solution**
4. Sélectionnez le fichier `.sln` du projet TravelPlann

### B. Vérifier les packages NuGet

Dans Visual Studio, ouvrez la **Console du Gestionnaire de package** :

```powershell
# Vérifiez que ces packages sont installés :
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.Extensions.Configuration.Json
Install-Package CommunityToolkit.Maui
Install-Package Microsoft.Maui.Controls.Maps
```

### C. Configurer appsettings.json

1. Dans le projet MAUI, créez ou modifiez le fichier `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost,1433;Database=TravelPlanner;User Id=sa_travelplann;Password=VotreMotDePasseComplexe123!;TrustServerCertificate=True;"
  }
}
```

**Variantes selon votre configuration Windows :**

* **Instance par défaut :** `Server=localhost,1433;` ou `Server=.;`
* **Instance nommée :** `Server=localhost\\SQLEXPRESS,1433;`
* **Authentification Windows :** `Server=localhost,1433;Database=TravelPlanner;Integrated Security=True;TrustServerCertificate=True;`
* **LocalDB :** `Server=(localdb)\\mssqllocaldb;Database=TravelPlanner;Trusted_Connection=true;TrustServerCertificate=True;`

2. **Marquer appsettings.json comme ressource embarquée** :
   * Clic droit sur `appsettings.json` dans l'Explorateur de solutions
   * **Propriétés** → **Action de génération** : `Ressource incorporée`

### D. Configuration MauiProgram.cs pour Windows

Remplacez le contenu de `MauiProgram.cs` par :

```csharp
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
            .UseMauiMaps()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => 
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuration pour Windows - lecture du fichier appsettings.json
        string connectionString;
  
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("TravelPlannMauiApp.appsettings.json");
  
            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
          
                connectionString = config.GetConnectionString("TravelPlannConnectionString");
                Debug.WriteLine($"Chaîne de connexion depuis appsettings.json: {connectionString}");
            }
            else
            {
                // Fallback pour Windows avec valeurs par défaut
                connectionString = "Server=localhost,1433;Database=TravelPlanner;User Id=sa_travelplann;Password=VotreMotDePasseComplexe123!;TrustServerCertificate=True;";
                Debug.WriteLine("Utilisation de la chaîne de connexion par défaut");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lecture appsettings.json: {ex.Message}");
            // Chaîne de connexion de secours pour Windows
            connectionString = "Server=localhost,1433;Database=TravelPlanner;User Id=sa_travelplann;Password=VotreMotDePasseComplexe123!;TrustServerCertificate=True;";
        }

        // Configuration Entity Framework avec retry policy pour Windows
        builder.Services.AddDbContextFactory<TravelPlannDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60); // Timeout plus long pour Windows
            });
  
#if DEBUG
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.LogTo(message => Debug.WriteLine($"EF: {message}"));
#endif
        });

        // Services 
        builder.Services.AddScoped<IActiviteService, ActiviteService>();
        builder.Services.AddScoped<IHebergementService, HebergementService>();
        builder.Services.AddScoped<IVoyageService, VoyageService>();
        builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ISessionService, SessionService>();

        // ViewModels
        builder.Services.AddTransient<VoyageViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<VoyageDetailsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AddVoyageViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<MainPageViewModel>();

        // HttpClient pour APIs externes
        builder.Services.AddHttpClient("WeatherApi", client =>
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org");
            client.DefaultRequestHeaders.Add("User-Agent", "TravelPlannApp/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
  
        // Pages
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
  
        // Test de connexion spécifique Windows
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                Debug.WriteLine("=== TEST DE CONNEXION WINDOWS ===");
      
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TravelPlannDbContext>>();
                Debug.WriteLine("✓ DbContextFactory obtenu");
      
                using var context = dbFactory.CreateDbContext();
      
                // Test de connexion avec détails pour Windows
                var canConnect = context.Database.CanConnect();
                Debug.WriteLine($"✓ Connexion DB: {canConnect}");
      
                if (canConnect)
                {
                    // Vérification des tables essentielles
                    var tableNames = new[] { "Utilisateur", "Voyage", "Activite", "Hebergement" };
                    foreach (var tableName in tableNames)
                    {
                        try
                        {
                            var sql = $"SELECT COUNT(*) FROM {tableName}";
                            var count = context.Database.SqlQueryRaw<int>(sql).FirstOrDefault();
                            Debug.WriteLine($"✓ Table {tableName}: {count} enregistrements");
                        }
                        catch (Exception tableEx)
                        {
                            Debug.WriteLine($"❌ Erreur table {tableName}: {tableEx.Message}");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("❌ ATTENTION: Connexion à la base de données impossible");
                    Debug.WriteLine("Vérifiez que :");
                    Debug.WriteLine("1. SQL Server est démarré");
                    Debug.WriteLine("2. La base TravelPlanner existe");
                    Debug.WriteLine("3. L'utilisateur sa_travelplann a les bons droits");
                    Debug.WriteLine("4. Le port 1433 est ouvert");
                }
      
                var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
                Debug.WriteLine("✓ UtilisateurService obtenu");
      
                Debug.WriteLine("=== CONFIGURATION TERMINÉE ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERREUR de configuration: {ex.Message}");
                Debug.WriteLine($"Type: {ex.GetType().Name}");
      
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Cause: {ex.InnerException.Message}");
                }
      
                // Messages d'aide spécifiques Windows
                if (ex.Message.Contains("login failed"))
                {
                    Debug.WriteLine("→ Vérifiez l'utilisateur et mot de passe SQL");
                }
                else if (ex.Message.Contains("server was not found"))
                {
                    Debug.WriteLine("→ Vérifiez que SQL Server est démarré");
                }
                else if (ex.Message.Contains("database") && ex.Message.Contains("does not exist"))
                {
                    Debug.WriteLine("→ Exécutez le script de création de la base");
                }
            }
        }

        return app;
    }   
}
```

## 3. Création de la base de données

### A. Utiliser les migrations Entity Framework

Dans la **Console du Gestionnaire de package** de Visual Studio :

```powershell
# Si vous n'avez pas encore de migrations
Add-Migration InitialCreate

# Appliquer les migrations
Update-Database
```

### B. Vérifier la création des tables

Dans **SQL Server Management Studio** :

1. Connectez-vous à votre serveur
2. Expandez **Bases de données** → **TravelPlanner**
3. Expandez **Tables** - vous devriez voir :
   * `Activite`
   * `ClassementVoyageur`
   * `GroupeVoyage`
   * `Hebergement`
   * `MembreGroupe`
   * `Utilisateur`
   * `Voyage`
   * Tables de liaison (`ActiviteVoyage`, `HebergementVoyage`, etc.)

---

## 4. Test et débogage

### A. Lancer l'application

1. Dans Visual Studio, sélectionnez la plateforme cible :
   * **Windows Machine** pour tester sur Windows
   * **Android Emulator** si configuré
2. Appuyez sur **F5** ou cliquez sur **Démarrer le débogage**

### B. Vérifier les logs de connexion

Dans la **Fenêtre de sortie** de Visual Studio :

1. Sélectionnez **Afficher la sortie à partir de : Débogage**
2. Recherchez ces messages :
   ```
   Test de connexion DB: TrueServices configurés correctement
   ```

### C. Problèmes courants et solutions

#### Erreur "Login failed for user"

* Vérifiez l'utilisateur et mot de passe dans `appsettings.json`
* Assurez-vous que l'authentification mixte est activée

#### Erreur "Server not found"

* Vérifiez que SQL Server est démarré
* Testez la connexion : `telnet localhost 1433`
* Vérifiez le nom de l'instance

#### Erreur "Database does not exist"

```sql
-- Créez manuellement la base dans SSMS :
CREATE DATABASE TravelPlanner;
```

#### Problème de migrations

```powershell
# Supprimer et recréer les migrations
Remove-Migration
Add-Migration InitialCreate
Update-Database
```

---

## 5. Configuration avancée (optionnel)

### A. SQL Server Express LocalDB (alternative légère)

Si vous préférez utiliser LocalDB :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=(localdb)\\mssqllocaldb;Database=TravelPlanner;Trusted_Connection=true;TrustServerCertificate=True;"
  }
}
```

### B. Docker Desktop (si vous voulez reproduire ma config)

1. Installez **Docker Desktop for Windows**
2. Utilisez cette commande :

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=1235OHdf%e" -p 1433:1433 --name sql_server_travelplann -d mcr.microsoft.com/mssql/server:2019-latest
```

### C. Seed data (données de test)

Vous pouvez ajouter des données de test dans `Program.cs` :

```csharp
// Après la création du contexte
if (!context.Utilisateurs.Any())
{
    context.Utilisateurs.Add(new Utilisateur 
    { 
        Nom = "Test", 
        Prenom = "User", 
        Email = "test@test.com",
        MotDePasse = "test123" // En production, hashé !
    });
    await context.SaveChangesAsync();
}
```

---

## 6. Structure finale attendue

Votre projet devrait avoir cette structure :

```
TravelPlannMauiApp/
├── BU/
│   └── Services/
├── DAL/
│   └── DB/
├── Pages/
├── ViewModels/
├── appsettings.json (Build Action: Embedded Resource)
├── MauiProgram.cs
└── TravelPlannMauiApp.csproj
```

---

## Support

Si vous rencontrez des problèmes :

1. Vérifiez les logs dans la **Fenêtre de sortie** de Visual Studio
2. Testez la connexion SQL avec SSMS
3. Vérifiez que tous les services SQL Server sont démarrés

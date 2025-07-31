

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

## 3. Configuration Console App Test (optionnel)

Si vous voulez tester la connexion avec une app console, créez `Program.cs` :

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using DAL.DB;
using BU.Services;

namespace TravelPlannConsoleAppTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Test TravelPlanner - Windows ===");
        Console.WriteLine("Initialisation...");

        try
        {
            // Configuration pour Windows
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Chaîne de connexion avec fallback
            var connectionString = config.GetConnectionString("TravelPlannConnectionString") ??
                "Server=localhost,1433;Database=TravelPlanner;User Id=sa_travelplann;Password=VotreMotDePasseComplexe123!;TrustServerCertificate=True;";

            Console.WriteLine($"Connexion: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

            // Configuration Entity Framework
            var options = new DbContextOptionsBuilder<TravelPlannDbContext>()
                .UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                    sqlOptions.CommandTimeout(30);
                })
                .EnableDetailedErrors()
                .Options;

            await using var context = new TravelPlannDbContext(options);
          
            // Test de connexion
            Console.WriteLine("Test de connexion...");
            var canConnect = await context.Database.CanConnectAsync();
          
            if (!canConnect)
            {
                Console.WriteLine("❌ Impossible de se connecter à la base de données !");
                Console.WriteLine("Vérifiez votre configuration SQL Server.");
                return;
            }
          
            Console.WriteLine("✓ Connexion réussie !");

            // Test des services
            var voyageService = new VoyageService(context);
            var utilisateurService = new UtilisateurService(context);

            // Afficher les utilisateurs
            Console.WriteLine("\n=== SOLUTIONS POSSIBLES ===");
            if (ex.Message.Contains("login failed"))
            {
                Console.WriteLine("→ Problème d'authentification SQL Server");
                Console.WriteLine("  1. Vérifiez l'utilisateur et mot de passe");
                Console.WriteLine("  2. Assurez-vous que l'authentification mixte est activée");
            }
            else if (ex.Message.Contains("server was not found"))
            {
                Console.WriteLine("→ SQL Server non accessible");
                Console.WriteLine("  1. Vérifiez que SQL Server est démarré");
                Console.WriteLine("  2. Testez: telnet localhost 1433");
            }
            else if (ex.Message.Contains("database") && ex.Message.Contains("does not exist"))
            {
                Console.WriteLine("→ Base de données manquante");
                Console.WriteLine("  1. Exécutez le script de création dans SSMS");
            }
        }

        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
```

Avec l'`appsettings.json` correspondant :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost,1433;Database=TravelPlanner;User Id=sa_travelplann;Password=VotreMotDePasseComplexe123!;TrustServerCertificate=True;"
  }
}
```

---

## 4. Test et validation

### A. Lancer l'application MAUI

1. Dans Visual Studio, sélectionnez la plateforme cible :
   * **Windows Machine** pour desktop Windows
   * **Android Emulator** si configuré
2. Appuyez sur **F5** ou **Démarrer le débogage**

### B. Vérifier les logs dans Visual Studio

1. **Affichage** → **Sortie**
2. Dans "Afficher la sortie à partir de", sélectionnez **Débogage**
3. Recherchez ces messages de succès :
   ```
   ✓ DbContextFactory obtenu✓ Connexion DB: True✓ Table Utilisateur: 2 enregistrements✓ Table Voyage: 0 enregistrements✓ UtilisateurService obtenu=== CONFIGURATION TERMINÉE ===
   ```

### C. Test de l'app console (optionnel)

1. Créez un nouveau projet console dans la solution
2. Ajoutez les références vers DAL et BU
3. Copiez le code `Program.cs` ci-dessus
4. Exécutez pour valider la connexion

---

## 5. Résolution des problèmes courants Windows

### A. Erreur "Login failed for user"

```bash
# Dans SSMS, exécutez :
USE master;
ALTER LOGIN sa_travelplann WITH PASSWORD = 'NouveauMotDePasse123!';
ALTER LOGIN sa_travelplann ENABLE;
```

### B. Erreur "Server not found"

1. Vérifiez les services Windows :
   * `services.msc` → Cherchez "SQL Server"
   * Démarrez le service si arrêté
2. Testez le port : `telnet localhost 1433`
3. Vérifiez le pare-feu Windows

### C. Erreur "Database does not exist"

* Re-exécutez le script de création SQL complet
* Vérifiez que vous êtes connecté au bon serveur

### D. Problème de performance

Dans `MauiProgram.cs`, ajustez les timeouts :

```csharp
sqlOptions.CommandTimeout(120); // 2 minutes
sqlOptions.EnableRetryOnFailure(maxRetryCount: 10);
```

### E. Erreur "appsettings.json not found"

1. Vérifiez que le fichier est bien dans le projet MAUI
2. Propriétés → **Action de génération** : `Ressource incorporée`
3. Rebuild complet de la solution

---

## 6. Configuration avancée Windows

### A. Utilisation de Windows Authentication

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost;Database=TravelPlanner;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

### B. Instance SQL Server nommée

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost\\SQLEXPRESS,1433;Database=TravelPlanner;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

### C. SQL Server LocalDB (plus simple)

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=(localdb)\\mssqllocaldb;Database=TravelPlanner;Trusted_Connection=true;TrustServerCertificate=True;"
  }
}
```

---

## 7. Validation finale

### Checklist de vérification :

* [ ] SQL Server est démarré et accessible
* [ ] Base TravelPlanner créée avec toutes les tables
* [ ] Utilisateur sa_travelplann créé avec droits appropriés
* [ ] appsettings.json configuré et marqué comme ressource incorporée
* [ ] Tous les packages NuGet installés
* [ ] L'application MAUI se lance sans erreur
* [ ] Les logs montrent une connexion réussie
* [ ] Les données de test sont visibles

### En cas de succès, vous devriez voir :

```
=== TEST DE CONNEXION WINDOWS ===
✓ DbContextFactory obtenu
✓ Connexion DB: True
✓ Table Utilisateur: 2 enregistrements
✓ Table Voyage: 0 enregistrements
✓ Table Activite: 3 enregistrements
✓ Table Hebergement: 3 enregistrements
✓ UtilisateurService obtenu
=== CONFIGURATION TERMINÉE ===
```

---

## 8. Prochaines étapes

Une fois l'installation validée :

1. **Testez l'authentification** avec admin@test.com / admin123
2. **Créez votre premier voyage** via l'interface
3. **Explorez les fonctionnalités** de cartographie et planification
4. **Personnalisez** selon vos besoins

 **Félicitations ! Votre application TravelPlanner est opérationnelle sur Windows ! 🎉** n=== UTILISATEURS ===");
var users = await utilisateurService.GetAllAsync();
if (users.Any())
{
foreach (var user in users)
{
Console.WriteLine($"- {user.Prenom} {user.Nom} ({user.Email})");
}
}
else
{
Console.WriteLine("Aucun utilisateur trouvé.");
}

```
        // Afficher les voyages
        Console.WriteLine("\n=== VOYAGES ===");
        var voyages = await voyageService.GetVoyagesAsync();
      
        if (voyages.Any())
        {
            foreach (var voyage in voyages)
            {
                Console.WriteLine($"- {voyage.NomVoyage}");
                Console.WriteLine($"  Description: {voyage.Description ?? "Aucune"}");
                Console.WriteLine($"  Dates: {voyage.DateDebut:dd/MM/yyyy} au {voyage.DateFin:dd/MM/yyyy}");
                Console.WriteLine($"  Créateur: ID {voyage.UtilisateurId}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("Aucun voyage trouvé.");
            Console.WriteLine("La base semble vide - c'est normal lors de la première installation.");
        }

        // Test statistiques
        Console.WriteLine("\n=== STATISTIQUES ===");
        var userCount = users.Count();
        var voyageCount = voyages.Count();
      
        Console.WriteLine($"Utilisateurs: {userCount}");
        Console.WriteLine($"Voyages: {voyageCount}");
      
        Console.WriteLine("\n✓ Tous les tests passés avec succès !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ ERREUR: {ex.Message}");
        Console.WriteLine("\nDétails techniques:");
        Console.WriteLine(ex.ToString());
      
        Console.WriteLine("\# Instructions d'installation - TravelPlann MAUI App
```

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

```sql
-- Dans SSMS, exécutez ces commandes :
USE master;
GO

-- Créer la base de données
CREATE DATABASE TravelPlanner;
GO

-- Créer le login et l'utilisateur
CREATE LOGIN sa_travelplann WITH PASSWORD = 'VotreMotDePasseComplexe123!';
GO

USE TravelPlanner;
GO

CREATE USER sa_travelplann FOR LOGIN sa_travelplann;
GO

-- Donner les droits complets
ALTER ROLE db_owner ADD MEMBER sa_travelplann;
GO
```

### D. Configurer le port et TCP/IP

1. Dans **SQL Server Configuration Manager**
2. Allez dans **SQL Server Network Configuration** → **Protocols for MSSQLSERVER**
3. Activez **TCP/IP** (clic droit → Enable)
4. Double-cliquez sur **TCP/IP** → Onglet **IP Addresses**
5. Scrollez jusqu'à **IPAll** et définissez :
   * **TCP Dynamic Ports** : (laisser vide)
   * **TCP Port** : `1433`
6. Redémarrez le service SQL Server

### E. Créer les tables avec le script SQL

Dans SSMS, connectez-vous à la base **TravelPlanner** et exécutez ce script complet :

```sql
-- Script de création des tables TravelPlanner
USE TravelPlanner;
GO

-- Table Activite
CREATE TABLE dbo.Activite (
    ActiviteID int NOT NULL IDENTITY(1,1),
    Nom nvarchar(100) NOT NULL,
    Description nvarchar(max) NULL,
    Localisation nvarchar(255) NULL
);
GO

ALTER TABLE dbo.Activite ADD CONSTRAINT PK__Activite__BE3FB865F23B2E86 PRIMARY KEY (ActiviteID);
GO

-- Table Utilisateur
CREATE TABLE dbo.Utilisateur (
    UtilisateurID int NOT NULL IDENTITY(1,1),
    Nom nvarchar(100) NOT NULL,
    Prenom nvarchar(100) NOT NULL,
    Email nvarchar(255) NOT NULL,
    MotDePasse nvarchar(255) NOT NULL,
    PointsRecompenses int NOT NULL DEFAULT 0
);
GO

ALTER TABLE dbo.Utilisateur ADD CONSTRAINT PK__Utilisat__6CB6AE1F1218C985 PRIMARY KEY (UtilisateurID);
GO

ALTER TABLE dbo.Utilisateur ADD CONSTRAINT UQ_Utilisateur_Email UNIQUE (Email);
GO

-- Table Voyage
CREATE TABLE dbo.Voyage (
    VoyageID int NOT NULL IDENTITY(1,1),
    NomVoyage nvarchar(100) NOT NULL,
    Description nvarchar(max) NULL,
    DateDebut date NOT NULL,
    DateFin date NOT NULL,
    EstComplete bit NOT NULL DEFAULT 0,
    EstArchive bit NOT NULL DEFAULT 0,
    UtilisateurID int NOT NULL
);
GO

ALTER TABLE dbo.Voyage ADD CONSTRAINT PK__Voyage__577D73A343C0B05F PRIMARY KEY (VoyageID);
GO

ALTER TABLE dbo.Voyage ADD CONSTRAINT FK_Voyage_Utilisateur 
    FOREIGN KEY (UtilisateurID) REFERENCES dbo.Utilisateur(UtilisateurID);
GO

-- Table Hebergement
CREATE TABLE dbo.Hebergement (
    HebergementID int NOT NULL IDENTITY(1,1),
    Nom nvarchar(100) NOT NULL,
    TypeHebergement nvarchar(50) NULL,
    Cout decimal(10,2) NULL,
    DateDebut date NULL,
    DateFin date NULL,
    Adresse nvarchar(255) NULL
);
GO

ALTER TABLE dbo.Hebergement ADD CONSTRAINT PK__Hebergem__35A3F6B1A87E8D30 PRIMARY KEY (HebergementID);
GO

-- Table de liaison ActiviteVoyage
CREATE TABLE dbo.ActiviteVoyage (
    ActiviteID int NOT NULL,
    VoyageID int NOT NULL
);
GO

ALTER TABLE dbo.ActiviteVoyage ADD CONSTRAINT PK_ActiviteVoyage PRIMARY KEY (ActiviteID, VoyageID);
GO

ALTER TABLE dbo.ActiviteVoyage ADD CONSTRAINT FK_ActiviteVoyage_Activite 
    FOREIGN KEY (ActiviteID) REFERENCES dbo.Activite(ActiviteID) ON DELETE CASCADE;
GO

ALTER TABLE dbo.ActiviteVoyage ADD CONSTRAINT FK_ActiviteVoyage_Voyage 
    FOREIGN KEY (VoyageID) REFERENCES dbo.Voyage(VoyageID) ON DELETE CASCADE;
GO

-- Table de liaison HebergementVoyage
CREATE TABLE dbo.HebergementVoyage (
    VoyageID int NOT NULL,
    HebergementID int NOT NULL
);
GO

ALTER TABLE dbo.HebergementVoyage ADD CONSTRAINT PK_HebergementVoyage PRIMARY KEY (VoyageID, HebergementID);
GO

ALTER TABLE dbo.HebergementVoyage ADD CONSTRAINT FK_HebergementVoyage_Hebergement 
    FOREIGN KEY (HebergementID) REFERENCES dbo.Hebergement(HebergementID) ON DELETE CASCADE;
GO

ALTER TABLE dbo.HebergementVoyage ADD CONSTRAINT FK_HebergementVoyage_Voyage 
    FOREIGN KEY (VoyageID) REFERENCES dbo.Voyage(VoyageID) ON DELETE CASCADE;
GO

-- Table GroupeVoyage
CREATE TABLE dbo.GroupeVoyage (
    GroupeID int NOT NULL IDENTITY(1,1),
    NomGroupe nvarchar(100) NOT NULL,
    DateCreation date DEFAULT GETDATE()
);
GO

ALTER TABLE dbo.GroupeVoyage ADD CONSTRAINT PK__GroupeVo__5C811B3078FA0CDF PRIMARY KEY (GroupeID);
GO

-- Table MembreGroupe
CREATE TABLE dbo.MembreGroupe (
    MembreGroupeID int NOT NULL IDENTITY(1,1),
    UtilisateurID int NOT NULL,
    GroupeID int NOT NULL,
    Role nvarchar(50) NOT NULL,
    DateAdhesion date DEFAULT GETDATE()
);
GO

ALTER TABLE dbo.MembreGroupe ADD CONSTRAINT PK__MembreGr__DED3D73B96357064 PRIMARY KEY (MembreGroupeID);
GO

ALTER TABLE dbo.MembreGroupe ADD CONSTRAINT FK_Membre_Groupe 
    FOREIGN KEY (GroupeID) REFERENCES dbo.GroupeVoyage(GroupeID);
GO

ALTER TABLE dbo.MembreGroupe ADD CONSTRAINT FK_Membre_Utilisateur 
    FOREIGN KEY (UtilisateurID) REFERENCES dbo.Utilisateur(UtilisateurID);
GO

-- Table OrganisationVoyage (relation many-to-many Utilisateur-Voyage)
CREATE TABLE dbo.OrganisationVoyage (
    UtilisateurID int NOT NULL,
    VoyageID int NOT NULL
);
GO

ALTER TABLE dbo.OrganisationVoyage ADD CONSTRAINT PK_OrganisationVoyage PRIMARY KEY (UtilisateurID, VoyageID);
GO

ALTER TABLE dbo.OrganisationVoyage ADD CONSTRAINT FK_OrganisationVoyage_Utilisateur 
    FOREIGN KEY (UtilisateurID) REFERENCES dbo.Utilisateur(UtilisateurID) ON DELETE CASCADE;
GO

ALTER TABLE dbo.OrganisationVoyage ADD CONSTRAINT FK_OrganisationVoyage_Voyage 
    FOREIGN KEY (VoyageID) REFERENCES dbo.Voyage(VoyageID) ON DELETE CASCADE;
GO

-- Table NiveauRecompense
CREATE TABLE dbo.NiveauRecompense (
    NiveauRecompenseID int NOT NULL IDENTITY(1,1),
    NomNiveau nvarchar(50) NOT NULL,
    PointsRequis int NOT NULL,
    Avantages nvarchar(max) NULL
);
GO

ALTER TABLE dbo.NiveauRecompense ADD CONSTRAINT PK__NiveauRe__04A74635554B36E3 PRIMARY KEY (NiveauRecompenseID);
GO

-- Table PointsRecompense
CREATE TABLE dbo.PointsRecompense (
    PointsRecompenseID int NOT NULL IDENTITY(1,1),
    PointsGagnes int NOT NULL,
    DateObtention date DEFAULT GETDATE(),
    UtilisateurID int NOT NULL,
    NiveauRecompenseID int NULL
);
GO

ALTER TABLE dbo.PointsRecompense ADD CONSTRAINT PK__PointsRe__9A7FC267F35BD8D3 PRIMARY KEY (PointsRecompenseID);
GO

ALTER TABLE dbo.PointsRecompense ADD CONSTRAINT FK_Points_Utilisateur 
    FOREIGN KEY (UtilisateurID) REFERENCES dbo.Utilisateur(UtilisateurID);
GO

ALTER TABLE dbo.PointsRecompense ADD CONSTRAINT FK_Points_NiveauRecompense 
    FOREIGN KEY (NiveauRecompenseID) REFERENCES dbo.NiveauRecompense(NiveauRecompenseID);
GO

-- Table ClassementVoyageur
CREATE TABLE dbo.ClassementVoyageur (
    ClassementID int NOT NULL IDENTITY(1,1),
    UtilisateurID int NOT NULL,
    Rang int NOT NULL,
    NombreVoyages int NOT NULL,
    DistanceTotale decimal(10,2) NOT NULL
);
GO

ALTER TABLE dbo.ClassementVoyageur ADD CONSTRAINT PK__Classeme__63F085DD50156D31 PRIMARY KEY (ClassementID);
GO

ALTER TABLE dbo.ClassementVoyageur ADD CONSTRAINT FK_Classement_Utilisateur 
    FOREIGN KEY (UtilisateurID) REFERENCES dbo.Utilisateur(UtilisateurID);
GO

-- Table ReservationHebergement
CREATE TABLE dbo.ReservationHebergement (
    ReservationID int NOT NULL IDENTITY(1,1),
    HebergementID int NOT NULL,
    StatutReservation nvarchar(50) NOT NULL,
    NumConfirmation nvarchar(50) NOT NULL
);
GO

ALTER TABLE dbo.ReservationHebergement ADD CONSTRAINT PK__Reservat__B7EE5F04BCBEFAA4 PRIMARY KEY (ReservationID);
GO

ALTER TABLE dbo.ReservationHebergement ADD CONSTRAINT FK_Reservation_Hebergement 
    FOREIGN KEY (HebergementID) REFERENCES dbo.Hebergement(HebergementID);
GO

-- Insertion de données de test
INSERT INTO dbo.Utilisateur (Nom, Prenom, Email, MotDePasse, PointsRecompenses)
VALUES 
    ('Admin', 'Test', 'admin@test.com', 'admin123', 0),
    ('User', 'Demo', 'user@test.com', 'user123', 50);
GO

INSERT INTO dbo.Activite (Nom, Description, Localisation)
VALUES 
    ('Visite du Louvre', 'Visite du célèbre musée parisien', 'Paris, France'),
    ('Tour Eiffel', 'Montée au sommet de la Tour Eiffel', 'Paris, France'),
    ('Plage de Nice', 'Détente sur la Côte d''Azur', 'Nice, France');
GO

INSERT INTO dbo.Hebergement (Nom, TypeHebergement, Cout, Adresse)
VALUES 
    ('Hôtel Ritz Paris', 'Hôtel', 450.00, '15 Place Vendôme, Paris'),
    ('Airbnb Montmartre', 'Appartement', 120.00, 'Montmartre, Paris'),
    ('Camping Côte d''Azur', 'Camping', 35.00, 'Nice, France');
GO

PRINT 'Base de données TravelPlanner créée avec succès !';
```

---

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

### C. Configurer la chaîne de connexion

1. Dans le projet, trouvez le fichier `appsettings.json`
2. Modifiez la chaîne de connexion selon votre configuration :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost,1433;Database=TravelPlanner;User Id=sa_travelplann;Password=VotreMotDePasseComplexe123!;TrustServerCertificate=True;"
  }
}
```

**Variantes selon votre installation :**

* **Instance par défaut :** `Server=localhost,1433;` ou `Server=.;`
* **Instance nommée :** `Server=localhost\\SQLEXPRESS,1433;` (remplacez SQLEXPRESS par le nom de votre instance)
* **Authentification Windows :** `Server=localhost,1433;Database=TravelPlanner;Integrated Security=True;TrustServerCertificate=True;`

### D. Marquer appsettings.json comme ressource embarquée

1. Dans l' **Explorateur de solutions** , clic droit sur `appsettings.json`
2. **Propriétés**
3. **Action de génération** : `Ressource incorporée`
4. **Copier dans le répertoire de sortie** : `Ne pas copier`

---

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
4. Consultez la documentation officielle Microsoft pour .NET MAUI

**Bonne chance avec votre projet TravelPlann ! 🚀**

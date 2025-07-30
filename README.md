# Instructions d'installation - TravelPlann MAUI App

## Configuration Windows avec Visual Studio et SQL Server local

### Prérequis

* Windows 10/11
* Visual Studio 2022 (Community, Professional ou Enterprise)
* SQL Server (Express, Developer ou Standard) installé localement
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

### C. Créer un utilisateur SQL

```sql
-- Dans SSMS, exécutez ces commandes :
USE master;
GO

-- Créer le login
CREATE LOGIN sa_travelplann WITH PASSWORD = 'VotreMotDePasseComplexe123!';
GO

-- Créer la base de données
CREATE DATABASE TravelPlanner;
GO

-- Donner les droits à l'utilisateur
USE TravelPlanner;
GO
ALTER AUTHORIZATION ON DATABASE::TravelPlanner TO sa_travelplann;
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

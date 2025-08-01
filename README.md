# Guide complet d'installation TravelPlanner - Windows

## SQL Server Express 2022 + Visual Studio

## 🎯 Vue d'ensemble

Ce guide vous permettra d'installer et configurer complètement l'application TravelPlanner avec SQL Server Express 2022 sur Windows.

---

## 1. Prérequis

### A. Logiciels nécessaires

* **Windows 10/11**
* **Visual Studio 2022** (Community, Professional ou Enterprise)
* **SQL Server Express 2022** installé
* **SQL Server Management Studio (SSMS)** - [Télécharger ici](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
* **.NET 8 SDK** ou supérieur

### B. Vérifier SQL Server Express

1. Ouvrez **Services Windows** (services.msc)
2. Vérifiez que `SQL Server (SQLEXPRESS)` est **démarré**
3. Si arrêté, clic droit → **Démarrer**

---

## 2. Configuration SQL Server Express 2022

### A. Activer TCP/IP (obligatoire)

1. Ouvrez **SQL Server Configuration Manager**
   * Tapez "SQL Server Configuration Manager" dans le menu Démarrer
   * Ou naviguez vers `C:\Windows\SysWOW64\SQLServerManager16.msc`
2. Allez dans **SQL Server Network Configuration** → **Protocols for SQLEXPRESS**
3. Clic droit sur **TCP/IP** → **Enable** (Activer)
4. Double-cliquez sur **TCP/IP** → Onglet **IP Addresses**
5. Scrollez jusqu'à **IPAll** et définissez :
   * **TCP Dynamic Ports** : (laisser vide)
   * **TCP Port** : `1433`
6. **Redémarrez le service SQL Server (SQLEXPRESS)** dans Services Windows

### B. Tester la connexion

1. Ouvrez **SQL Server Management Studio (SSMS)**
2. Paramètres de connexion :
   * **Server name** : `localhost\SQLEXPRESS` ou `.\SQLEXPRESS`
   * **Authentication** : Windows Authentication
3. Cliquez **Connect**

> ⚠️  **Si la connexion échoue** , essayez : `(local)\SQLEXPRESS` ou `127.0.0.1\SQLEXPRESS`

---

## 3. Création de la base de données

### A. Créer la base TravelPlanner

Dans SSMS, exécutez :

```sql
-- Créer la base de données
USE master;
GO

CREATE DATABASE TravelPlanner;
GO

-- Vérifier la création
SELECT name FROM sys.databases WHERE name = 'TravelPlanner';
GO

-- Utiliser la nouvelle base
USE TravelPlanner;
GO

PRINT '✅ Base de données TravelPlanner créée avec succès !';
```

### B. Exécuter le script complet des tables

1. Dans SSMS, assurez-vous d'être sur la base **TravelPlanner**
2. Ouvrez le fichier **`TravelPlanner.sql`** disponible à la racine de la solution **TravelPlannSolution**
3. Copiez et exécutez tout le contenu de ce script dans SSMS
4. Ce script créera toutes les tables avec leurs données de test

---

## 4. Configuration Visual Studio

### A. Ouvrir le projet TravelPlanner

1. Ouvrez **Visual Studio 2022**
2. **Fichier** → **Ouvrir** → **Projet/Solution**
3. Sélectionnez le fichier `.sln` du projet TravelPlanner

### B. Configurer la chaîne de connexion

1. Ouvrez le fichier `appsettings.json` dans le projet
2. Remplacez le contenu par :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost\\SQLEXPRESS;Database=TravelPlanner;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### C. Configurer appsettings.json

1. Clic droit sur `appsettings.json` dans l'Explorateur de solutions
2. **Propriétés**
3. **Build Action** : `Embedded Resource`
4. **Copy to Output Directory** : `Do not copy`

### D. Vérifier les packages NuGet

Dans la  **Console du Gestionnaire de package** , vérifiez :

```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.Extensions.Configuration.Json
Install-Package CommunityToolkit.Maui
Install-Package Microsoft.Maui.Controls.Maps
```

---

## 5. Test et validation

### A. Lancer l'application

1. Dans Visual Studio, sélectionnez **Windows Machine** comme plateforme cible
2. Appuyez sur **F5** ou cliquez sur **Démarrer le débogage**

### B. Vérifier les logs de connexion

1. Dans Visual Studio : **Affichage** → **Sortie**
2. Sélectionnez "Afficher la sortie à partir de :  **Debug** "
3. Recherchez ces messages de succès :

```
✅ DbContextFactory obtenu
✅ Connexion DB: True
✅ Table Utilisateur: X enregistrements
✅ Table Voyage: Y enregistrements
=== CONFIGURATION TERMINÉE ===
```

### C. Test rapide de données

Dans SSMS, exécutez pour vérifier :

```sql
USE TravelPlanner;

-- Compter les enregistrements
SELECT 'Utilisateur' as Table_Name, COUNT(*) as Count FROM Utilisateur
UNION ALL
SELECT 'Voyage', COUNT(*) FROM Voyage
UNION ALL
SELECT 'Activite', COUNT(*) FROM Activite
UNION ALL
SELECT 'Hebergement', COUNT(*) FROM Hebergement;
```

---

## 6. Chaînes de connexion alternatives

### Si l'authentification Windows ne fonctionne pas :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost\\SQLEXPRESS,1433;Database=TravelPlanner;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### Si vous voulez utiliser un utilisateur SQL :

```json
{
  "ConnectionStrings": {
    "TravelPlannConnectionString": "Server=localhost\\SQLEXPRESS;Database=TravelPlanner;User Id=sa;Password=VotreMotDePasse;TrustServerCertificate=True;"
  }
}
```

---

## 7. Résolution des problèmes courants

| Problème                           | Solution                                                                                                     |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| **"Server not found"**        | Vérifiez que SQL Server (SQLEXPRESS) est démarré`<br>`Essayez `.\SQLEXPRESS`ou `(local)\SQLEXPRESS` |
| **"Login failed"**            | Utilisez Windows Authentication`<br>`Vérifiez les droits de votre utilisateur Windows                     |
| **"Database does not exist"** | Re-exécutez le script de création de base                                                                  |
| **"TCP/IP not enabled"**      | Activez TCP/IP dans SQL Server Configuration Manager                                                         |
| **Port 1433 bloqué**         | Vérifiez le pare-feu Windows                                                                                |

---

**🚀 Votre application TravelPlanner est maintenant opérationnelle !**

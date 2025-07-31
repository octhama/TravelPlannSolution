using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BU.Services;

public class UtilisateurService : IUtilisateurService
{
    private readonly IDbContextFactory<TravelPlannDbContext> _dbContextFactory;

    public UtilisateurService(IDbContextFactory<TravelPlannDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Utilisateur?> AuthenticateAsync(string email, string motDePasse)
    {
        try
        {
            Debug.WriteLine($"=== DÉBUT AuthenticateAsync ===");
            Debug.WriteLine($"Email recherché: '{email}'");
            Debug.WriteLine($"Mot de passe fourni: '{motDePasse}'");

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            Debug.WriteLine("Contexte DB créé avec succès via factory");

            var canConnect = await context.Database.CanConnectAsync();
            Debug.WriteLine($"Connexion DB possible: {canConnect}");

            if (!canConnect)
            {
                Debug.WriteLine("ERREUR: Impossible de se connecter à la base de données");
                throw new InvalidOperationException("Connexion à la base de données impossible");
            }

            var user = await context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                Debug.WriteLine("Utilisateur non trouvé avec cet email");
                return null;
            }

            Debug.WriteLine($"Utilisateur trouvé: {user.Prenom} {user.Nom}");
            Debug.WriteLine($"Mot de passe en DB: '{user.MotDePasse}'");

            bool isPasswordHashed = IsPasswordHashed(user.MotDePasse);
            Debug.WriteLine($"Le mot de passe semble hashé: {isPasswordHashed}");

            bool isAuthenticated = false;

            if (isPasswordHashed)
            {
                isAuthenticated = VerifyPassword(motDePasse, user.MotDePasse);
                Debug.WriteLine("Utilisation de la vérification avec hash");
            }
            else
            {
                isAuthenticated = (motDePasse == user.MotDePasse);
                Debug.WriteLine("Utilisation de la comparaison directe");
            }

            Debug.WriteLine($"Authentification réussie: {isAuthenticated}");

            if (isAuthenticated)
            {
                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERREUR dans AuthenticateAsync: {ex.Message}");
            Debug.WriteLine($"Type d'exception: {ex.GetType().Name}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }

            throw;
        }
        finally
        {
            Debug.WriteLine($"=== FIN AuthenticateAsync ===");
        }
    }

    public async Task<Utilisateur> CreateUserAsync(string nom, string prenom, string email, string motDePasse)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            if (await EmailExistsAsync(email))
            {
                throw new InvalidOperationException("Cette adresse email est déjà utilisée.");
            }

            var utilisateur = new Utilisateur
            {
                Nom = nom,
                Prenom = prenom,
                Email = email,
                MotDePasse = HashPassword(motDePasse),
                PointsRecompenses = 0
            };

            context.Utilisateurs.Add(utilisateur);
            await context.SaveChangesAsync();

            return utilisateur;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur CreateUserAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<Utilisateur?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Utilisateurs
                .Include(u => u.Voyages)
                .Include(u => u.MembreGroupes)
                .Include(u => u.PointsRecompensesNavigation)
                .FirstOrDefaultAsync(u => u.UtilisateurId == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur GetByIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<Utilisateur?> GetByEmailAsync(string email)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur GetByEmailAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Utilisateur>> GetAllAsync()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Utilisateurs
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenom)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur GetAllAsync: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(Utilisateur utilisateur)
    {
        try
        {
            Debug.WriteLine($"=== DÉBUT UpdateAsync ===");
            Debug.WriteLine($"Utilisateur ID: {utilisateur.UtilisateurId}");
            Debug.WriteLine($"Nom: {utilisateur.Nom}");
            Debug.WriteLine($"Prénom: {utilisateur.Prenom}");
            Debug.WriteLine($"Email: {utilisateur.Email}");

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            
            // Vérifier que l'utilisateur existe en base
            var existingUser = await context.Utilisateurs
                .FirstOrDefaultAsync(u => u.UtilisateurId == utilisateur.UtilisateurId);
            
            if (existingUser == null)
            {
                Debug.WriteLine($"ERREUR: Utilisateur avec ID {utilisateur.UtilisateurId} non trouvé en base");
                throw new InvalidOperationException($"Utilisateur avec ID {utilisateur.UtilisateurId} non trouvé");
            }

            Debug.WriteLine($"Utilisateur existant trouvé: {existingUser.Nom} {existingUser.Prenom}");

            // Vérifier si l'email a changé et s'il n'est pas déjà utilisé par un autre utilisateur
            if (existingUser.Email != utilisateur.Email)
            {
                var emailAlreadyExists = await context.Utilisateurs
                    .AnyAsync(u => u.Email == utilisateur.Email && u.UtilisateurId != utilisateur.UtilisateurId);
                
                if (emailAlreadyExists)
                {
                    Debug.WriteLine($"ERREUR: Email {utilisateur.Email} déjà utilisé par un autre utilisateur");
                    throw new InvalidOperationException("Cette adresse email est déjà utilisée par un autre utilisateur");
                }
            }

            // Mettre à jour les propriétés
            existingUser.Nom = utilisateur.Nom;
            existingUser.Prenom = utilisateur.Prenom;
            existingUser.Email = utilisateur.Email;
            
            // Si le mot de passe a été modifié, le hasher
            if (!string.IsNullOrEmpty(utilisateur.MotDePasse) && 
                existingUser.MotDePasse != utilisateur.MotDePasse)
            {
                Debug.WriteLine("Mise à jour du mot de passe");
                // Vérifier si le nouveau mot de passe est déjà hashé
                if (!IsPasswordHashed(utilisateur.MotDePasse))
                {
                    existingUser.MotDePasse = HashPassword(utilisateur.MotDePasse);
                    Debug.WriteLine("Mot de passe hashé");
                }
                else
                {
                    existingUser.MotDePasse = utilisateur.MotDePasse;
                    Debug.WriteLine("Mot de passe déjà hashé");
                }
            }

            // Marquer l'entité comme modifiée
            context.Entry(existingUser).State = EntityState.Modified;
            
            var changes = await context.SaveChangesAsync();
            Debug.WriteLine($"Nombre de changements sauvegardés: {changes}");

            if (changes == 0)
            {
                Debug.WriteLine("ATTENTION: Aucun changement détecté lors de la sauvegarde");
            }
            else
            {
                Debug.WriteLine("Mise à jour réussie");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERREUR UpdateAsync: {ex.Message}");
            Debug.WriteLine($"Type: {ex.GetType().Name}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            
            throw;
        }
        finally
        {
            Debug.WriteLine($"=== FIN UpdateAsync ===");
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var utilisateur = await context.Utilisateurs.FindAsync(id);
            if (utilisateur != null)
            {
                context.Utilisateurs.Remove(utilisateur);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur DeleteAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Utilisateurs
                .AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur EmailExistsAsync: {ex.Message}");
            throw;
        }
    }

    // Méthode pour vérifier si un email existe pour un autre utilisateur
    public async Task<bool> EmailExistsForOtherUserAsync(string email, int currentUserId)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Utilisateurs
                .AnyAsync(u => u.Email == email && u.UtilisateurId != currentUserId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur EmailExistsForOtherUserAsync: {ex.Message}");
            throw;
        }
    }

    public async Task AddPointsAsync(int utilisateurId, int points)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var utilisateur = await context.Utilisateurs.FindAsync(utilisateurId);
            if (utilisateur != null)
            {
                utilisateur.PointsRecompenses += points;

                var pointsRecompense = new PointsRecompense
                {
                    UtilisateurId = utilisateurId,
                    PointsGagnes = points,
                    DateObtention = DateOnly.FromDateTime(DateTime.Now)
                };

                context.PointsRecompenses.Add(pointsRecompense);
                await context.SaveChangesAsync();

                await UpdateClassementAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur AddPointsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ClassementVoyageur>> GetClassementAsync()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.ClassementVoyageurs
                .Include(c => c.Utilisateur)
                .OrderBy(c => c.Rang)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur GetClassementAsync: {ex.Message}");
            throw;
        }
    }

    private async Task UpdateClassementAsync()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var utilisateurs = await context.Utilisateurs
                .Include(u => u.Voyages)
                .ToListAsync();

            // Supprimer l'ancien classement
            context.ClassementVoyageurs.RemoveRange(context.ClassementVoyageurs);

            // Créer le nouveau classement
            var classement = utilisateurs
                .Select(u => new ClassementVoyageur
                {
                    UtilisateurId = u.UtilisateurId,
                    NombreVoyages = u.Voyages.Count,
                    DistanceTotale = 0,
                    Rang = 0
                })
                .OrderByDescending(c => c.NombreVoyages)
                .ThenByDescending(c => c.DistanceTotale)
                .ToList();

            for (int i = 0; i < classement.Count; i++)
            {
                classement[i].Rang = i + 1;
            }

            context.ClassementVoyageurs.AddRange(classement);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur UpdateClassementAsync: {ex.Message}");
            throw;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }

    private bool IsPasswordHashed(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length != 44)
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(password, @"^[A-Za-z0-9+/]*={0,2}$");
    }
}
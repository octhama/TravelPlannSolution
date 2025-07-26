using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace BU.Services;

public class UtilisateurService : IUtilisateurService
{
    private readonly TravelPlannDbContext _context;

    public UtilisateurService(TravelPlannDbContext context)
    {
        _context = context;
    }

    public async Task<Utilisateur?> AuthenticateAsync(string email, string motDePasse)
    {
        try
        {
            Debug.WriteLine($"=== DÉBUT AuthenticateAsync ===");
            Debug.WriteLine($"Email recherché: '{email}'");
            Debug.WriteLine($"Mot de passe fourni: '{motDePasse}'");

            var user = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                Debug.WriteLine("Utilisateur non trouvé avec cet email");
                return null;
            }

            Debug.WriteLine($"Utilisateur trouvé: {user.Prenom} {user.Nom}");
            Debug.WriteLine($"Mot de passe en DB: '{user.MotDePasse}'");

            // TEMPORAIRE: Vérifier si le mot de passe en DB semble être en clair ou hashé
            bool isPasswordHashed = IsPasswordHashed(user.MotDePasse);
            Debug.WriteLine($"Le mot de passe semble hashé: {isPasswordHashed}");

            bool isAuthenticated = false;

            if (isPasswordHashed)
            {
                // Utiliser la vérification avec hash
                isAuthenticated = VerifyPassword(motDePasse, user.MotDePasse);
                Debug.WriteLine("Utilisation de la vérification avec hash");
            }
            else
            {
                // Comparaison directe (mot de passe en clair)
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
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            Debug.WriteLine($"=== FIN AuthenticateAsync ===");
        }
    }

    public async Task<Utilisateur> CreateUserAsync(string nom, string prenom, string email, string motDePasse)
    {
        if (await EmailExistsAsync(email))
        {
            throw new InvalidOperationException("Cette adresse email est déjà utilisée.");
        }

        var utilisateur = new Utilisateur
        {
            Nom = nom,
            Prenom = prenom,
            Email = email,
            MotDePasse = HashPassword(motDePasse), // Les nouveaux utilisateurs auront des mots de passe hashés
            PointsRecompenses = 0
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return utilisateur;
    }

    public async Task<Utilisateur?> GetByIdAsync(int id)
    {
        return await _context.Utilisateurs
            .Include(u => u.Voyages)
            .Include(u => u.MembreGroupes)
            .Include(u => u.PointsRecompensesNavigation)
            .FirstOrDefaultAsync(u => u.UtilisateurId == id);
    }

    public async Task<Utilisateur?> GetByEmailAsync(string email)
    {
        return await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<Utilisateur>> GetAllAsync()
    {
        return await _context.Utilisateurs
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();
    }

    public async Task UpdateAsync(Utilisateur utilisateur)
    {
        _context.Utilisateurs.Update(utilisateur);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var utilisateur = await _context.Utilisateurs.FindAsync(id);
        if (utilisateur != null)
        {
            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Utilisateurs
            .AnyAsync(u => u.Email == email);
    }

    public async Task AddPointsAsync(int utilisateurId, int points)
    {
        var utilisateur = await _context.Utilisateurs.FindAsync(utilisateurId);
        if (utilisateur != null)
        {
            utilisateur.PointsRecompenses += points;

            var pointsRecompense = new PointsRecompense
            {
                UtilisateurId = utilisateurId,
                PointsGagnes = points,
                DateObtention = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.PointsRecompenses.Add(pointsRecompense);
            await _context.SaveChangesAsync();

            await UpdateClassementAsync();
        }
    }

    public async Task<List<ClassementVoyageur>> GetClassementAsync()
    {
        return await _context.ClassementVoyageurs
            .Include(c => c.Utilisateur)
            .OrderBy(c => c.Rang)
            .ToListAsync();
    }

    private async Task UpdateClassementAsync()
    {
        var utilisateurs = await _context.Utilisateurs
            .Include(u => u.Voyages)
            .ToListAsync();

        // Supprimer l'ancien classement
        _context.ClassementVoyageurs.RemoveRange(_context.ClassementVoyageurs);

        // Créer le nouveau classement
        var classement = utilisateurs
            .Select(u => new ClassementVoyageur
            {
                UtilisateurId = u.UtilisateurId,
                NombreVoyages = u.Voyages.Count,
                DistanceTotale = 0, // À calculer selon vos besoins
                Rang = 0
            })
            .OrderByDescending(c => c.NombreVoyages)
            .ThenByDescending(c => c.DistanceTotale)
            .ToList();

        for (int i = 0; i < classement.Count; i++)
        {
            classement[i].Rang = i + 1;
        }

        _context.ClassementVoyageurs.AddRange(classement);
        await _context.SaveChangesAsync();
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

    // Méthode pour détecter si un mot de passe est hashé ou en clair
    private bool IsPasswordHashed(string password)
    {
        // Un hash SHA256 en Base64 fait toujours 44 caractères
        // Et ne contient que des caractères alphanumériques + / et =
        if (password.Length != 44)
            return false;

        // Vérifier si le string contient uniquement des caractères Base64 valides
        return System.Text.RegularExpressions.Regex.IsMatch(password, @"^[A-Za-z0-9+/]*={0,2}$");
    }
}
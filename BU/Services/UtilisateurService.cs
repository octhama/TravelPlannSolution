using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        var user = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null && VerifyPassword(motDePasse, user.MotDePasse))
        {
            return user;
        }

        return null;
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
            MotDePasse = HashPassword(motDePasse),
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
}
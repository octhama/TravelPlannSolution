using BU.Models;
using DAL.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BU.Services
{
    public class UtilisateurService : IUtilisateurService
    {
        private readonly TravelPlannDbContext _context;
        private readonly ILogger<UtilisateurService> _logger;

        public UtilisateurService(TravelPlannDbContext context, ILogger<UtilisateurService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Utilisateur> GetUtilisateurParEmailAsync(string email)
        {
            try
            {
                return await _context.Utilisateurs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'utilisateur par email: {Email}", email);
                return null;
            }
        }

        public async Task<Utilisateur> GetUtilisateurParIdAsync(int id)
        {
            try
            {
                return await _context.Utilisateurs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UtilisateurID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'utilisateur par ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> AjouterUtilisateurAsync(Utilisateur utilisateur)
        {
            try
            {
                await _context.Utilisateurs.AddAsync(utilisateur);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout d'utilisateur: {Email}", utilisateur?.Email);
                return false;
            }
        }

        public async Task<bool> MettreAJourUtilisateurAsync(Utilisateur utilisateur)
        {
            try
            {
                _context.Utilisateurs.Update(utilisateur);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour d'utilisateur: {Id}", utilisateur?.UtilisateurID);
                return false;
            }
        }

        public async Task<bool> SupprimerUtilisateurAsync(int id)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs.FindAsync(id);
                if (utilisateur != null)
                {
                    utilisateur.EstActif = false; // Soft delete
                    var result = await _context.SaveChangesAsync();
                    return result > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression d'utilisateur: {Id}", id);
                return false;
            }
        }

        public async Task<List<Voyage>> GetVoyagesUtilisateurAsync(int utilisateurId)
        {
            try
            {
                return await _context.Voyages
                    .AsNoTracking()
                    .Where(v => v.UtilisateurId == utilisateurId)
                    .OrderByDescending(v => v.DateDebut)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des voyages pour l'utilisateur: {Id}", utilisateurId);
                return new List<Voyage>();
            }
        }
    }
}
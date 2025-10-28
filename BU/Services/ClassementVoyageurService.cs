using DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace BU.Services
{
    public class ClassementVoyageurService : IClassementVoyageurService
    {
        private readonly TravelPlannDbContext _context;

        public ClassementVoyageurService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClassementVoyageur>> GetClassementAsync()
        {
            return await _context.ClassementVoyageurs
                .Include(c => c.Utilisateur)
                .OrderBy(c => c.Rang)
                .ToListAsync();
        }

        public async Task<ClassementVoyageur> GetClassementByUtilisateurAsync(int utilisateurId)
        {
            var classement = await _context.ClassementVoyageurs
                .Include(c => c.Utilisateur)
                .FirstOrDefaultAsync(c => c.UtilisateurId == utilisateurId);
            return classement ?? throw new KeyNotFoundException($"Classement for utilisateur {utilisateurId} not found");
        }

        public async Task<bool> UpdateClassementAsync()
        {
            try
            {
                // Supprimer l'ancien classement
                _context.ClassementVoyageurs.RemoveRange(_context.ClassementVoyageurs);

                // Recalculer le classement basé sur le nombre de voyages et la distance totale
                var classement = await _context.Utilisateurs
                    .Select(u => new ClassementVoyageur
                    {
                        UtilisateurId = u.UtilisateurId,
                        Rang = 0, // Sera défini après le tri
                        NombreVoyages = u.VoyagesCreated.Count,
                        DistanceTotale = u.ClassementVoyageurs.Any() ? u.ClassementVoyageurs.First().DistanceTotale : 0
                    })
                    .OrderByDescending(c => c.NombreVoyages)
                    .ThenByDescending(c => c.DistanceTotale)
                    .ToListAsync();

                // Assigner les rangs
                int rang = 1;
                foreach (var item in classement)
                {
                    item.Rang = rang++;
                }

                _context.ClassementVoyageurs.AddRange(classement);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetClassementAsync()
        {
            try
            {
                _context.ClassementVoyageurs.RemoveRange(_context.ClassementVoyageurs);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
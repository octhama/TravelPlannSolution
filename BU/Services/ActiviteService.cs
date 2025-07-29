using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BU.Services
{
    public class ActiviteService : IActiviteService
    {
        private readonly TravelPlannDbContext _context;

        public ActiviteService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<Activite>> GetAllActivitesAsync()
        {
            return await _context.Activites.ToListAsync();
        }

        public async Task<Activite> AddActiviteAsync(Activite activite)
        {
            await _context.Activites.AddAsync(activite);
            await _context.SaveChangesAsync();
            return activite;
        }

        public async Task<IEnumerable<Activite>> GetActivitesByVoyageAsync(int voyageId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Recherche des activités pour le voyage {voyageId}");

                // Utiliser la relation many-to-many via la table ActiviteVoyage
                var activites = await _context.Activites
                    .Where(a => a.Voyages.Any(v => v.VoyageId == voyageId))
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Trouvé {activites?.Count() ?? 0} activités pour le voyage {voyageId}");

                return activites ?? new List<Activite>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la récupération des activités pour le voyage {voyageId}: {ex.Message}");
                return new List<Activite>();
            }
        }

        public async Task DeleteActiviteAsync(int activiteId)
        {
            try
            {
                var activite = await _context.Activites.FindAsync(activiteId);
                if (activite != null)
                {
                    _context.Activites.Remove(activite);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DB Error: {ex}");
                throw;
            }
        }
    }
}
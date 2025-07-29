using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BU.Services
{
    public class HebergementService : IHebergementService
    {
        private readonly TravelPlannDbContext _context;

        public HebergementService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<Hebergement>> GetAllHebergementsAsync()
        {
            return await _context.Hebergements.ToListAsync();
        }

        public async Task<Hebergement> AddHebergementAsync(Hebergement hebergement)
        {
            await _context.Hebergements.AddAsync(hebergement);
            await _context.SaveChangesAsync();
            return hebergement;
        }

        public async Task<IEnumerable<Hebergement>> GetHebergementsByVoyageAsync(int voyageId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Recherche des hébergements pour le voyage {voyageId}");

                // Utiliser la relation many-to-many via la table HebergementVoyage
                var hebergements = await _context.Hebergements
                    .Where(h => h.Voyages.Any(v => v.VoyageId == voyageId))
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Trouvé {hebergements?.Count() ?? 0} hébergements pour le voyage {voyageId}");

                return hebergements ?? new List<Hebergement>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la récupération des hébergements pour le voyage {voyageId}: {ex.Message}");
                return new List<Hebergement>();
            }
        }

        public async Task DeleteHebergementAsync(int hebergementId)
        {
            try
            {
                var hebergement = await _context.Hebergements.FindAsync(hebergementId);
                if (hebergement != null)
                {
                    _context.Hebergements.Remove(hebergement);
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
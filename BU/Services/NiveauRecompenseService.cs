using DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace BU.Services
{
    public class NiveauRecompenseService : INiveauRecompenseService
    {
        private readonly TravelPlannDbContext _context;

        public NiveauRecompenseService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<NiveauRecompense>> GetNiveauxAsync()
        {
            return await _context.NiveauRecompenses
                .OrderBy(n => n.PointsRequis)
                .ToListAsync();
        }

        public async Task<NiveauRecompense> GetNiveauByIdAsync(int niveauId)
        {
            var niveau = await _context.NiveauRecompenses
                .FirstOrDefaultAsync(n => n.NiveauRecompenseId == niveauId);
            return niveau ?? throw new KeyNotFoundException($"Niveau with id {niveauId} not found");
        }

        public async Task<NiveauRecompense> GetNiveauByPointsAsync(int points)
        {
            var niveau = await _context.NiveauRecompenses
                .Where(n => n.PointsRequis <= points)
                .OrderByDescending(n => n.PointsRequis)
                .FirstOrDefaultAsync();
            return niveau ?? throw new InvalidOperationException("No niveau found for the given points");
        }

        public async Task<NiveauRecompense> CreateNiveauAsync(NiveauRecompense niveau)
        {
            _context.NiveauRecompenses.Add(niveau);
            await _context.SaveChangesAsync();
            return niveau;
        }

        public async Task<NiveauRecompense> UpdateNiveauAsync(NiveauRecompense niveau)
        {
            _context.NiveauRecompenses.Update(niveau);
            await _context.SaveChangesAsync();
            return niveau;
        }

        public async Task<bool> DeleteNiveauAsync(int niveauId)
        {
            var niveau = await _context.NiveauRecompenses.FindAsync(niveauId);
            if (niveau == null)
                return false;

            _context.NiveauRecompenses.Remove(niveau);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
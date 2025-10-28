using DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace BU.Services
{
    public class PointsRecompenseService : IPointsRecompenseService
    {
        private readonly TravelPlannDbContext _context;

        public PointsRecompenseService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<PointsRecompense>> GetPointsHistoryAsync(int utilisateurId)
        {
            return await _context.PointsRecompenses
                .Where(p => p.UtilisateurId == utilisateurId)
                .Include(p => p.Utilisateur)
                .OrderByDescending(p => p.DateObtention)
                .ToListAsync();
        }

        public async Task<PointsRecompense> GetPointsByIdAsync(int pointsId)
        {
            var points = await _context.PointsRecompenses
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.PointsRecompenseId == pointsId);
            return points ?? throw new KeyNotFoundException($"Points with id {pointsId} not found");
        }

        public async Task<PointsRecompense> AddPointsAsync(int utilisateurId, int points, string description)
        {
            var pointsRecompense = new PointsRecompense
            {
                UtilisateurId = utilisateurId,
                PointsGagnes = points,
                DateObtention = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.PointsRecompenses.Add(pointsRecompense);
            await _context.SaveChangesAsync();

            // Mettre à jour le total des points de l'utilisateur
            var utilisateur = await _context.Utilisateurs.FindAsync(utilisateurId);
            if (utilisateur != null)
            {
                utilisateur.PointsRecompenses += points;
                await _context.SaveChangesAsync();
            }

            return pointsRecompense;
        }

        public async Task<int> GetTotalPointsAsync(int utilisateurId)
        {
            return await _context.PointsRecompenses
                .Where(p => p.UtilisateurId == utilisateurId)
                .SumAsync(p => p.PointsGagnes);
        }

        public async Task<List<PointsRecompense>> GetPointsByDateRangeAsync(int utilisateurId, DateTime startDate, DateTime endDate)
        {
            var start = DateOnly.FromDateTime(startDate);
            var end = DateOnly.FromDateTime(endDate);
            return await _context.PointsRecompenses
                .Where(p => p.UtilisateurId == utilisateurId &&
                           p.DateObtention >= start &&
                           p.DateObtention <= end)
                .Include(p => p.Utilisateur)
                .OrderByDescending(p => p.DateObtention)
                .ToListAsync();
        }
    }
}
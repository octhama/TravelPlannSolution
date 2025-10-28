using DAL.DB;

namespace BU.Services
{
    public interface IPointsRecompenseService
    {
        Task<List<PointsRecompense>> GetPointsHistoryAsync(int utilisateurId);
        Task<PointsRecompense> GetPointsByIdAsync(int pointsId);
        Task<PointsRecompense> AddPointsAsync(int utilisateurId, int points, string description);
        Task<int> GetTotalPointsAsync(int utilisateurId);
        Task<List<PointsRecompense>> GetPointsByDateRangeAsync(int utilisateurId, DateTime startDate, DateTime endDate);
    }
}
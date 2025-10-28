using DAL.DB;

namespace BU.Services
{
    public interface INiveauRecompenseService
    {
        Task<List<NiveauRecompense>> GetNiveauxAsync();
        Task<NiveauRecompense> GetNiveauByIdAsync(int niveauId);
        Task<NiveauRecompense> GetNiveauByPointsAsync(int points);
        Task<NiveauRecompense> CreateNiveauAsync(NiveauRecompense niveau);
        Task<NiveauRecompense> UpdateNiveauAsync(NiveauRecompense niveau);
        Task<bool> DeleteNiveauAsync(int niveauId);
    }
}
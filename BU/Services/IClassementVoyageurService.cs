using DAL.DB;

namespace BU.Services
{
    public interface IClassementVoyageurService
    {
        Task<List<ClassementVoyageur>> GetClassementAsync();
        Task<ClassementVoyageur> GetClassementByUtilisateurAsync(int utilisateurId);
        Task<bool> UpdateClassementAsync();
        Task<bool> ResetClassementAsync();
    }
}
namespace BU.Services;
public interface IGroupeVoyageService
{
    Task<GroupeVoyage> CreateAsync(string nomGroupe);
    Task<List<GroupeVoyage>> GetAllAsync();
    Task<GroupeVoyage?> GetByIdAsync(int id);
    Task UpdateAsync(GroupeVoyage groupe);
    Task DeleteAsync(int id);
    Task<MembreGroupe> AddMembreAsync(int groupeId, int utilisateurId, string role);
    Task RemoveMembreAsync(int groupeId, int utilisateurId);
    Task<List<MembreGroupe>> GetMembresAsync(int groupeId);
    Task<List<GroupeVoyage>> GetGroupesByUtilisateurAsync(int utilisateurId);
}
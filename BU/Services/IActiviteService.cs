namespace BU.Services
{
    public interface IActiviteService
    {
        Task<List<Activite>> GetAllActivitesAsync();
        Task<Activite> AddActiviteAsync(Activite activite);
        Task DeleteActiviteAsync(int activiteId);
        Task<IEnumerable<Activite>> GetActivitesByVoyageAsync(int voyageId);

    }
}
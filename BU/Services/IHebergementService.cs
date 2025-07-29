namespace BU.Services
{
    public interface IHebergementService
    {
        Task<List<Hebergement>> GetAllHebergementsAsync();
        Task<Hebergement> AddHebergementAsync(Hebergement hebergement);
        Task DeleteHebergementAsync(int hebergementId);
        Task<IEnumerable<Hebergement>> GetHebergementsByVoyageAsync(int voyageId);
           
    }
}
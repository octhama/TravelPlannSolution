using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.DB;

namespace BU.Services
{
    public interface IVoyageService
    {
        Task<List<Voyage>> GetVoyagesAsync(bool forceRefresh = false);
        Task AddVoyageAsync(Voyage voyage);
        Task UpdateVoyageAsync(Voyage voyage);
        Task DeleteVoyageAsync(int voyageId);
        Task<VoyageDetails> GetVoyageDetails(int voyageId);
        Task<VoyageDetails> GetVoyageDetailsAsync(int voyageId);
        Task AddActiviteToVoyageAsync(int voyageId, Activite activite);
        Task AddHebergementToVoyageAsync(int voyageId, Hebergement hebergement);
        Task<Voyage> GetVoyageByIdAsync(int voyageId); // Avec relations
        Task<List<Voyage>> GetVoyagesAsync(); // Avec relations
        Task RemoveActiviteFromVoyageAsync(int voyageId, int activiteId);
        Task RemoveHebergementFromVoyageAsync(int voyageId, int hebergementId);

        Task<Utilisateur> GetUtilisateurParEmail(string email);
        Task<bool> AjouterUtilisateur(Utilisateur utilisateur);
        Task<List<Voyage>> GetVoyagesParUtilisateur(int utilisateurId);    

    }
}
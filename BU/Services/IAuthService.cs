using DAL.DB;
using System.Collections.Generic;
using System.Threading.Tasks;
using BU.Models;

namespace BU.Services
{
    public interface IAuthService
    {
        Task<bool> Authentifier(string email, string motDePasse);
        Task<bool> Enregistrer(Utilisateur nouvelUtilisateur);
        Task<Utilisateur> GetUtilisateurActuel();
        Task Deconnecter();
        bool EstConnecte { get; }
    }

}
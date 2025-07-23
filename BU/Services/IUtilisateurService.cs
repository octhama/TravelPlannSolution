using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.DB;

namespace BU.Services
{
    public interface IUtilisateurService
    {
        Task<Utilisateur> GetUtilisateurParEmailAsync(string email);
        Task<Utilisateur> GetUtilisateurParIdAsync(int id);
        Task<bool> AjouterUtilisateurAsync(Utilisateur utilisateur);
        Task<bool> MettreAJourUtilisateurAsync(Utilisateur utilisateur);
        Task<bool> SupprimerUtilisateurAsync(int id);
        Task<List<Voyage>> GetVoyagesUtilisateurAsync(int utilisateurId);
    }
}
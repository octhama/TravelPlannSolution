using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BU.Services;

public interface IUtilisateurService
{
   Task<Utilisateur?> AuthenticateAsync(string email, string motDePasse);
    Task<Utilisateur> CreateUserAsync(string nom, string prenom, string email, string motDePasse);
    Task<Utilisateur?> GetByIdAsync(int id);
    Task<Utilisateur?> GetByEmailAsync(string email);
    Task<List<Utilisateur>> GetAllAsync();
    Task UpdateAsync(Utilisateur utilisateur);
    Task DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> EmailExistsForOtherUserAsync(string email, int currentUserId);
    Task AddPointsAsync(int utilisateurId, int points);
    Task<List<ClassementVoyageur>> GetClassementAsync();
}
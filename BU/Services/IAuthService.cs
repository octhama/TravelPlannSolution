using BU.Models;

namespace BU.Services
{
    public interface IAuthService
    {
        Task<AuthResult> AuthentiquerAsync(string email, string motDePasse);
        Task<AuthResult> EnregistrerAsync(Utilisateur nouvelUtilisateur);
        Task<Utilisateur> GetUtilisateurActuelAsync();
        Task DeconnecterAsync();
        bool EstConnecte { get; }
        event EventHandler<bool> EtatConnexionChanged;
    }

    public class AuthResult
    {
        public bool Succes { get; set; }
        public string MessageErreur { get; set; } = string.Empty;
        public Utilisateur Utilisateur { get; set; }
    }
}
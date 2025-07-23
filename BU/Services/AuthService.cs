using DAL.DB;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace BU.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUtilisateurService _utilisateurService;
        private readonly ILogger<AuthService> _logger;
        private Utilisateur _utilisateurActuel;

        public bool EstConnecte => _utilisateurActuel != null;
        public event EventHandler<bool> EtatConnexionChanged;

        public AuthService(IUtilisateurService utilisateurService, ILogger<AuthService> logger)
        {
            _utilisateurService = utilisateurService ?? throw new ArgumentNullException(nameof(utilisateurService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResult> AuthentiquerAsync(string email, string motDePasse)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
                {
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Email et mot de passe requis" 
                    };
                }

                var utilisateur = await _utilisateurService.GetUtilisateurParEmailAsync(email);
                
                if (utilisateur == null)
                {
                    _logger.LogWarning("Tentative de connexion avec email inexistant: {Email}", email);
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Email ou mot de passe incorrect" 
                    };
                }

                if (!utilisateur.EstActif)
                {
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Compte désactivé" 
                    };
                }

                if (!VerifierMotDePasse(motDePasse, utilisateur.MotDePasseHash))
                {
                    _logger.LogWarning("Mot de passe incorrect pour: {Email}", email);
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Email ou mot de passe incorrect" 
                    };
                }

                _utilisateurActuel = utilisateur;
                EtatConnexionChanged?.Invoke(this, true);
                
                _logger.LogInformation("Utilisateur connecté: {Email}", email);
                
                return new AuthResult 
                { 
                    Succes = true, 
                    Utilisateur = utilisateur 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'authentification pour {Email}", email);
                return new AuthResult 
                { 
                    Succes = false, 
                    MessageErreur = "Erreur lors de la connexion" 
                };
            }
        }

        public async Task<AuthResult> EnregistrerAsync(Utilisateur nouvelUtilisateur)
        {
            try
            {
                if (nouvelUtilisateur == null)
                {
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Données utilisateur requises" 
                    };
                }

                // Validation du modèle
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(nouvelUtilisateur);
                
                if (!Validator.TryValidateObject(nouvelUtilisateur, validationContext, validationResults, true))
                {
                    var erreurs = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = erreurs 
                    };
                }

                // Vérifier si l'email existe déjà
                var utilisateurExistant = await _utilisateurService.GetUtilisateurParEmailAsync(nouvelUtilisateur.Email);
                if (utilisateurExistant != null)
                {
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Un compte avec cet email existe déjà" 
                    };
                }

                // Valider le mot de passe
                if (!EstMotDePasseValide(nouvelUtilisateur.MotDePasseHash))
                {
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule et un chiffre" 
                    };
                }

                // Hasher le mot de passe
                nouvelUtilisateur.MotDePasseHash = HasherMotDePasse(nouvelUtilisateur.MotDePasseHash);
                nouvelUtilisateur.DateInscription = DateTime.Now;
                nouvelUtilisateur.EstActif = true;

                var succes = await _utilisateurService.AjouterUtilisateurAsync(nouvelUtilisateur);
                
                if (succes)
                {
                    _logger.LogInformation("Nouvel utilisateur enregistré: {Email}", nouvelUtilisateur.Email);
                    return new AuthResult 
                    { 
                        Succes = true, 
                        Utilisateur = nouvelUtilisateur 
                    };
                }
                else
                {
                    return new AuthResult 
                    { 
                        Succes = false, 
                        MessageErreur = "Erreur lors de l'enregistrement" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de {Email}", nouvelUtilisateur?.Email);
                return new AuthResult 
                { 
                    Succes = false, 
                    MessageErreur = "Erreur lors de l'enregistrement" 
                };
            }
        }

        public Task<Utilisateur> GetUtilisateurActuelAsync()
        {
            return Task.FromResult(_utilisateurActuel);
        }

        public Task DeconnecterAsync()
        {
            var etaitConnecte = _utilisateurActuel != null;
            _utilisateurActuel = null;
            
            if (etaitConnecte)
            {
                EtatConnexionChanged?.Invoke(this, false);
                _logger.LogInformation("Utilisateur déconnecté");
            }
            
            return Task.CompletedTask;
        }

        private string HasherMotDePasse(string motDePasse)
        {
            // Utilisation de BCrypt (recommandé en production)
            // Pour cette démo, utilisation de SHA256 avec salt
            using var sha256 = SHA256.Create();
            var salt = "TravelPlann2024"; // En production, utiliser un salt unique par utilisateur
            var saltedPassword = motDePasse + salt;
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifierMotDePasse(string motDePasse, string hashStocke)
        {
            return HasherMotDePasse(motDePasse) == hashStocke;
        }

        private bool EstMotDePasseValide(string motDePasse)
        {
            if (string.IsNullOrWhiteSpace(motDePasse) || motDePasse.Length < 8)
                return false;

            bool aMinuscule = motDePasse.Any(char.IsLower);
            bool aMajuscule = motDePasse.Any(char.IsUpper);
            bool aChiffre = motDePasse.Any(char.IsDigit);

            return aMinuscule && aMajuscule && aChiffre;
        }
    }
}
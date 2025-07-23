using System.ComponentModel.DataAnnotations;

namespace BU.Models
{
    public class Utilisateur
    {
        public int UtilisateurID { get; set; }
        
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string Nom { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string Prenom { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string MotDePasseHash { get; set; } = string.Empty;
        
        public int PointsRecompenses { get; set; } = 0;
        public DateTime DateInscription { get; set; } = DateTime.Now;
        public bool EstActif { get; set; } = true;
        
        // Propriétés calculées
        public string NomComplet => $"{Prenom} {Nom}";
    }
}
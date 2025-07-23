using System.Collections.Generic;
using DAL.DB;
using System.ComponentModel.DataAnnotations;

namespace BU.Models
{
    // BU/Models/Utilisateur.cs
    public class Utilisateur
    {
        public int UtilisateurID { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string MotDePasseHash { get; set; }
        public int PointsRecompenses { get; set; }
        public DateTime DateInscription { get; set; } = DateTime.Now;
    }
}
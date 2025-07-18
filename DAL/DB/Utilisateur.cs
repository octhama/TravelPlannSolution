using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class Utilisateur
{
    public int UtilisateurId { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MotDePasse { get; set; } = null!;

    public int PointsRecompenses { get; set; }

    public virtual ICollection<ClassementVoyageur> ClassementVoyageurs { get; set; } = new List<ClassementVoyageur>();

    public virtual ICollection<MembreGroupe> MembreGroupes { get; set; } = new List<MembreGroupe>();

    public virtual ICollection<PointsRecompense> PointsRecompensesNavigation { get; set; } = new List<PointsRecompense>();

    public virtual ICollection<Voyage> Voyages { get; set; } = new List<Voyage>();
}

using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class Voyage
{
    public int VoyageId { get; set; }

    public string NomVoyage { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly DateDebut { get; set; }

    public DateOnly DateFin { get; set; }

    public bool EstComplete { get; set; }

    public bool EstArchive { get; set; }
    public int UtilisateurId { get; set; }

    public virtual ICollection<Activite>? Activites { get; set; }
    public virtual ICollection<Hebergement>? Hebergements { get; set; }

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
    public virtual Utilisateur Utilisateur { get; set; } = null!;
}

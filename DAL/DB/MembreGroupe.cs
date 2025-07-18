using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class MembreGroupe
{
    public int MembreGroupeId { get; set; }

    public int UtilisateurId { get; set; }

    public int GroupeId { get; set; }

    public string Role { get; set; } = null!;

    public DateOnly DateAdhesion { get; set; }

    public virtual GroupeVoyage Groupe { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}

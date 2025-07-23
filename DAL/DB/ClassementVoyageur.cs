using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class ClassementVoyageur
{
    public int ClassementId { get; set; }

    public int UtilisateurId { get; set; }

    public int Rang { get; set; }

    public int NombreVoyages { get; set; }

    public decimal DistanceTotale { get; set; }

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}

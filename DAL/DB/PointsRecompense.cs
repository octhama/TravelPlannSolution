using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class PointsRecompense
{
    public int PointsRecompenseId { get; set; }

    public int PointsGagnes { get; set; }

    public DateOnly DateObtention { get; set; }

    public int UtilisateurId { get; set; }

    public int? NiveauRecompenseId { get; set; }

    public virtual NiveauRecompense? NiveauRecompense { get; set; }

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}

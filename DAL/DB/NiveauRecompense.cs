using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class NiveauRecompense
{
    public int NiveauRecompenseId { get; set; }

    public string NomNiveau { get; set; } = null!;

    public int PointsRequis { get; set; }

    public string? Avantages { get; set; }

    public virtual ICollection<PointsRecompense> PointsRecompenses { get; set; } = new List<PointsRecompense>();
}

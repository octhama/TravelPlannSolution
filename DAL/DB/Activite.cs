using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class Activite
{
    public int ActiviteId { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Voyage> Voyages { get; set; } = new List<Voyage>();
}

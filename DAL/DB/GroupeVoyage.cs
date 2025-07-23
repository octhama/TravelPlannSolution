using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class GroupeVoyage
{
    public int GroupeId { get; set; }

    public string NomGroupe { get; set; } = null!;

    public DateOnly? DateCreation { get; set; }

    public virtual ICollection<MembreGroupe> MembreGroupes { get; set; } = new List<MembreGroupe>();
}

using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class Hebergement
{
    public int HebergementId { get; set; }

    public string Nom { get; set; } = null!;

    public string? TypeHebergement { get; set; }

    public decimal? Cout { get; set; }

    public DateOnly? DateDebut { get; set; }

    public DateOnly? DateFin { get; set; }
    public string? Adresse { get; set; }

    public virtual ICollection<ReservationHebergement> ReservationHebergements { get; set; } = new List<ReservationHebergement>();

    public virtual ICollection<Voyage> Voyages { get; set; } = new List<Voyage>();
}
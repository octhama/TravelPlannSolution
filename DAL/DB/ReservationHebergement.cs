using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class ReservationHebergement
{
    public int ReservationId { get; set; }

    public int VoyageId { get; set; }

    public int HebergementId { get; set; }

    public DateOnly DateDebut { get; set; }

    public DateOnly DateFin { get; set; }

    public int NombrePersonnes { get; set; }

    public decimal PrixTotal { get; set; }

    public bool StatutReservation { get; set; }

    public string NumConfirmation { get; set; } = null!;

    public virtual Voyage Voyage { get; set; } = null!;
    public virtual Hebergement Hebergement { get; set; } = null!;
}

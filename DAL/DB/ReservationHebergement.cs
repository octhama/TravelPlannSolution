using System;
using System.Collections.Generic;

namespace DAL.DB;

public partial class ReservationHebergement
{
    public int ReservationId { get; set; }

    public int HebergementId { get; set; }

    public bool StatutReservation { get; set; }

    public string NumConfirmation { get; set; } = null!;

    public virtual Hebergement Hebergement { get; set; } = null!;
}

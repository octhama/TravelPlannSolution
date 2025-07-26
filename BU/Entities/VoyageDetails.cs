using System.Collections.Generic;
using DAL.DB;
using System.ComponentModel.DataAnnotations;

namespace BU.Entities
{
    public class VoyageDetails
    {
        public Voyage Voyage { get; set; }
        public List<Activite> Activites { get; set; }
        public List<Hebergement> Hebergements { get; set; }
    }
}
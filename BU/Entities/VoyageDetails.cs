using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BU.Entities
{
    public class VoyageDetails
    {
        public Voyage Voyage { get; set; }
        public List<Activite> Activites { get; set; } = new();
        public List<Hebergement> Hebergements { get; set; } = new();
    }
}
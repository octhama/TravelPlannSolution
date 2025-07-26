using DAL.DB;
using System.Collections.Generic;
using System.Threading.Tasks;
using BU.Entities;

namespace BU.Services
{
    public interface IHebergementService
    {
        Task<List<Hebergement>> GetAllHebergementsAsync();
        Task<Hebergement> AddHebergementAsync(Hebergement hebergement);
        Task DeleteHebergementAsync(int hebergementId);
           
    }
}
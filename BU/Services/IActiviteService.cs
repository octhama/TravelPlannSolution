using DAL.DB;
using System.Collections.Generic;
using System.Threading.Tasks;
using BU.Models;

namespace BU.Services
{
    public interface IActiviteService
    {
        Task<List<Activite>> GetAllActivitesAsync();
        Task<Activite> AddActiviteAsync(Activite activite);
        Task DeleteActiviteAsync(int activiteId);

    }
}
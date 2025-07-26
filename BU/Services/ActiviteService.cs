using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BU.Entities;

namespace BU.Services
{
    public class ActiviteService : IActiviteService
    {
        private readonly TravelPlannDbContext _context;

        public ActiviteService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<Activite>> GetAllActivitesAsync()
        {
            return await _context.Activites.ToListAsync();
        }

        public async Task<Activite> AddActiviteAsync(Activite activite)
        {
            await _context.Activites.AddAsync(activite);
            await _context.SaveChangesAsync();
            return activite;
        }

        public async Task DeleteActiviteAsync(int activiteId)
        {
            try
            {
                var activite = await _context.Activites.FindAsync(activiteId);
                if (activite != null)
                {
                    _context.Activites.Remove(activite);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DB Error: {ex}");
                throw;
            }
        }
    }
}
using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BU.Entities;

namespace BU.Services
{
    public class HebergementService : IHebergementService
    {
        private readonly TravelPlannDbContext _context;

        public HebergementService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<Hebergement>> GetAllHebergementsAsync()
        {
            return await _context.Hebergements.ToListAsync();
        }

        public async Task<Hebergement> AddHebergementAsync(Hebergement hebergement)
        {
            await _context.Hebergements.AddAsync(hebergement);
            await _context.SaveChangesAsync();
            return hebergement;
        }

        public async Task DeleteHebergementAsync(int hebergementId)
        {
            try
            {
                var hebergement = await _context.Hebergements.FindAsync(hebergementId);
                if (hebergement != null)
                {
                    _context.Hebergements.Remove(hebergement);
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
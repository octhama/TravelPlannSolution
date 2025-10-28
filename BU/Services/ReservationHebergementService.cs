using DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace BU.Services
{
    public class ReservationHebergementService : IReservationHebergementService
    {
        private readonly TravelPlannDbContext _context;

        public ReservationHebergementService(TravelPlannDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReservationHebergement>> GetReservationsAsync()
        {
            return await _context.ReservationHebergements
                .Include(r => r.Hebergement)
                .ToListAsync();
        }

        public async Task<List<ReservationHebergement>> GetReservationsByVoyageAsync(int voyageId)
        {
            // Note: ReservationHebergement is not directly linked to Voyage
            // This would require joining through HebergementVoyage table
            return new List<ReservationHebergement>();
        }

        public async Task<List<ReservationHebergement>> GetReservationsByUtilisateurAsync(int utilisateurId)
        {
            // Note: ReservationHebergement is not directly linked to Utilisateur
            return new List<ReservationHebergement>();
        }

        public async Task<ReservationHebergement> GetReservationByIdAsync(int reservationId)
        {
            var reservation = await _context.ReservationHebergements
                .Include(r => r.Hebergement)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
            return reservation ?? throw new KeyNotFoundException($"Reservation with id {reservationId} not found");
        }

        public async Task<ReservationHebergement> CreateReservationAsync(ReservationHebergement reservation)
        {
            _context.ReservationHebergements.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<ReservationHebergement> UpdateReservationAsync(ReservationHebergement reservation)
        {
            _context.ReservationHebergements.Update(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<bool> DeleteReservationAsync(int reservationId)
        {
            var reservation = await _context.ReservationHebergements.FindAsync(reservationId);
            if (reservation == null)
                return false;

            _context.ReservationHebergements.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
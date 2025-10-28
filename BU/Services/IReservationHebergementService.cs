using DAL.DB;

namespace BU.Services
{
    public interface IReservationHebergementService
    {
        Task<List<ReservationHebergement>> GetReservationsAsync();
        Task<List<ReservationHebergement>> GetReservationsByVoyageAsync(int voyageId);
        Task<List<ReservationHebergement>> GetReservationsByUtilisateurAsync(int utilisateurId);
        Task<ReservationHebergement> GetReservationByIdAsync(int reservationId);
        Task<ReservationHebergement> CreateReservationAsync(ReservationHebergement reservation);
        Task<ReservationHebergement> UpdateReservationAsync(ReservationHebergement reservation);
        Task<bool> DeleteReservationAsync(int reservationId);
    }
}
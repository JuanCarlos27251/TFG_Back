using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Repositories
{
    public interface IReservationRepository
    {
        Task<IEnumerable<ReservationDto>> GetByUserIdAsync(int userId);
        Task<ReservationDto?> GetByIdAsync(int id);
        Task<ReservationDto> CreateAsync(ReservationDtin dtin);
        Task<bool> UpdateStatusAsync(int id, ReservationStatus status);
        Task<bool> IsSpotAvailableAsync(int spotId, DateTime start, DateTime end);
    }
}
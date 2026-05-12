using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IReservationService
    {
    // Cliente
    Task<ReservationDto> CreateReservationAsync(ReservationDtin dtin);
    Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId);
    Task<bool> UpdateReservationAsync(int id, ReservationDtin dtin); 
    Task<bool> CancelReservationAsync(int id);
    
    // Management (Manager/Admin)
    Task<IEnumerable<ReservationDto>> GetByCompanyAsync(int companyId); // Para Managers
    Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(); // Para Admin
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    }
}
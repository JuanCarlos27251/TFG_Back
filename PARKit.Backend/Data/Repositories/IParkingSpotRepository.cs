using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Repositories
{
    public interface IParkingSpotRepository
    {
       Task<IEnumerable<ParkingSpotDto>> GetByParkingIdAsync(int parkingId);
        Task<ParkingSpotDto?> GetByIdAsync(int id);
        Task<ParkingSpotDto> AddAsync(ParkingSpotDtin dtin);
        Task<bool> UpdateStatusAsync(int id, SpotStatus status);
        Task<bool> DeleteAsync(int id);
    }
}
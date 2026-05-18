using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IParkingSpotService
    {
        Task<IEnumerable<ParkingSpotDto>> GetSpotsByParkingIdAsync(int parkingId);
        Task<ParkingSpotDto?> GetSpotByIdAsync(int id);
        Task<ParkingSpotDto> AddSpotAsync(ParkingSpotDtin dtin);
        Task<bool> UpdateSpotStatusAsync(int id, SpotStatus status);
        Task<bool> DeleteSpotAsync(int id);
    }
}
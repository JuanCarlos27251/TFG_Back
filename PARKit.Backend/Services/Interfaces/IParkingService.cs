using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IParkingService
    {
        //Coonsultas generales
        Task<IEnumerable<ParkingDto>> GetAllParkingsAsync(ParkingType? type = null, bool? onlyAvailable = null);
        Task<ParkingDto?> GetParkingByIdAsync(int id);

        //Consultas (Manager/Admin)
        Task<IEnumerable<ParkingDto>> GetByManagerIdAsync(int managerId);
        Task<ParkingDto> CreateParkingAsync(ParkingDtin dtin);
        Task<bool> UpdateParkingAsync(int id, ParkingDtin dtin);
        Task<bool> DeleteParkingAsync(int id);
    }
}
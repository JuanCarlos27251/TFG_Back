using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;

namespace PARKit.Backend.Repositories
{
    public interface IParkingRepository
    {
      Task<IEnumerable<ParkingDto>> GetAllAsync();
        Task<ParkingDto?> GetByIdAsync(int id);
        Task<IEnumerable<ParkingDto>> GetByCompanyIdAsync(int companyId);
        Task<ParkingDto> AddAsync(ParkingDtin dtin);
        Task<bool> UpdateAsync(int id, ParkingDtin dtin);
        Task<bool> DeleteAsync(int id);
    }
}
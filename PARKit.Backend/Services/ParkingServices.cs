using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class ParkingService : IParkingService
    {
      private readonly IParkingRepository _parkingRepository;
 
        public ParkingService(IParkingRepository parkingRepository)
        {
            _parkingRepository = parkingRepository;
        }
 
        public async Task<ParkingDto?> GetParkingByIdAsync(int id)
        {
            return await _parkingRepository.GetByIdAsync(id);
        }
 
        public async Task<IEnumerable<ParkingDto>> GetAllParkingsAsync(ParkingType? type = null, bool? onlyAvailable = null)
        {
            var parkings = await _parkingRepository.GetAllAsync();
 
            if (type.HasValue)
                parkings = parkings.Where(p => p.Type == type.Value);

            if (onlyAvailable == true)
                parkings = parkings.Where(p => p.AvailableSpots > 0);
 
            return parkings;
        }
 
        public async Task<IEnumerable<ParkingDto>> GetByManagerIdAsync(int managerId)
        {
            return await _parkingRepository.GetByCompanyIdAsync(managerId);
        }
 
        public async Task<ParkingDto> CreateParkingAsync(ParkingDtin dtin)
        {
            return await _parkingRepository.AddAsync(dtin);
        }
 
        public async Task<bool> UpdateParkingAsync(int id, ParkingDtin dtin)
        {
            return await _parkingRepository.UpdateAsync(id, dtin);
        }
 
        public async Task<bool> DeleteParkingAsync(int id)
        {
            return await _parkingRepository.DeleteAsync(id);
        }
    }
}
 
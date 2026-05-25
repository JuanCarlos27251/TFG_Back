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
        private readonly IParkingSpotRepository _parkingSpotRepository;
        private readonly ITarifRepository _tarifRepository;

       public ParkingService(IParkingRepository parkingRepository, IParkingSpotRepository parkingSpotRepository, ITarifRepository tarifRepository)
        {
            _parkingRepository = parkingRepository;
            _parkingSpotRepository = parkingSpotRepository;
            _tarifRepository = tarifRepository;
        }

        public async Task<ParkingDto?> GetParkingByIdAsync(int id)
        {
            var parkingEntity = await _parkingRepository.GetByIdAsync(id);
            if (parkingEntity == null) return null;

            var spots = await _parkingSpotRepository.GetByParkingIdAsync(id);
            var tarifs = await _tarifRepository.GetByParkingIdAsync(id);

            var dto = new ParkingDto
            {
                Id = parkingEntity.Id,
                CompanyId = parkingEntity.CompanyId,
                Name = parkingEntity.Name,
                Description = parkingEntity.Description,
                Address = parkingEntity.Address,
                Latitude = parkingEntity.Latitude,
                Longitude = parkingEntity.Longitude,
                Type = parkingEntity.Type,
                IsActive = parkingEntity.IsActive,
                ImageUrl = parkingEntity.ImageUrl,
                GeometryData = parkingEntity.GeometryData,
                CreatedAt = parkingEntity.CreatedAt,
                Spots = spots.ToList(),
                Tarifs = tarifs.ToList()
            };

            dto.AvailableSpots = dto.Spots.Count(s => s.Status == SpotStatus.Free);
            return dto;
        }

        public async Task<IEnumerable<ParkingDto>> GetAllParkingsAsync(ParkingType? type = null, bool? onlyAvailable = null)
        {
            var parkings = await _parkingRepository.GetAllAsync();

            // Filtro por tipo de parking (Public, Private, RegulatedSurface, etc.)
            if (type.HasValue)
                parkings = parkings.Where(p => p.Type == type.Value);

            // Filtro por disponibilidad: solo parkings con al menos una plaza libre
            if (onlyAvailable == true)
            {
                var enrichedList = new List<ParkingDto>();
                foreach (var p in parkings)
                {
                    var spots = await _parkingSpotRepository.GetByParkingIdAsync(p.Id);
                    var spotList = spots.ToList();
                    p.Spots = spotList;
                    p.AvailableSpots = spotList.Count(s => s.Status == SpotStatus.Free);
                    enrichedList.Add(p);
                }
                return enrichedList.Where(p => p.AvailableSpots > 0);
            }

            return parkings;
        }

        public async Task<IEnumerable<ParkingDto>> GetByManagerIdAsync(int managerId)
        {
            return await _parkingRepository.GetByCompanyIdAsync(managerId);
        }

        public async Task<bool> UpdateParkingAsync(int id, ParkingDtin dtin)
        {
            return await _parkingRepository.UpdateAsync(id, dtin);
        }

        public async Task<ParkingDto> CreateParkingAsync(ParkingDtin dtin)
        {
            return await _parkingRepository.AddAsync(dtin);
        }

        public async Task<bool> DeleteParkingAsync(int id)
        {
            return await _parkingRepository.DeleteAsync(id);
        }
    }
}
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

        public ParkingService(IParkingRepository parkingRepository,IParkingSpotRepository parkingSpotRepository,ITarifRepository tarifRepository )
        {
            _parkingRepository = parkingRepository;
            _parkingSpotRepository = parkingSpotRepository;
            _tarifRepository = tarifRepository;
        }

       public async Task<ParkingDto?> GetParkingByIdAsync(int id)
        {
            // 1. Buscamos el Parking (Usando tu repo de Parkings)
            var parkingEntity = await _parkingRepository.GetByIdAsync(id);
            if (parkingEntity == null) return null;

            // 2. Obtenemos los datos relacionados de sus propios repositorios
            var spots = await _parkingSpotRepository.GetByParkingIdAsync(id);
            var tarifs = await _tarifRepository.GetByParkingIdAsync(id);

            // 3. Mapeamos manualmente a tu ParkingDto
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
                
                // Asignamos las listas
                Spots = spots.ToList(),
                Tarifs = tarifs.ToList()
            };

            // 4. Calculamos las plazas libres (Usa SpotStatus.Available de tu Enum)
            dto.AvailableSpots = dto.Spots.Count(s => s.Status == SpotStatus.Free);

            return dto;
        }
        public async Task<IEnumerable<ParkingDto>> GetByManagerIdAsync(int managerId)
        {
            return await _parkingRepository.GetByCompanyIdAsync(managerId);
        }
        public async Task<bool> UpdateParkingAsync(int id, ParkingDtin dtin)
        {
            return await _parkingRepository.UpdateAsync(id, dtin);
        }

        public async Task<IEnumerable<ParkingDto>> GetAllParkingsAsync()
        {
            // Para el listado general, devolvemos los DTOs básicos
            return await _parkingRepository.GetAllAsync();
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
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class ParkingSpotService : IParkingSpotService
    {
        private readonly IParkingSpotRepository _spotRepository;

        public ParkingSpotService(IParkingSpotRepository spotRepository)
        {
            _spotRepository = spotRepository;
        }

        public async Task<IEnumerable<ParkingSpotDto>> GetSpotsByParkingIdAsync(int parkingId)
        {
            return await _spotRepository.GetByParkingIdAsync(parkingId);
        }

        public async Task<ParkingSpotDto?> GetSpotByIdAsync(int id)
        {
            return await _spotRepository.GetByIdAsync(id);
        }

        public async Task<ParkingSpotDto> AddSpotAsync(ParkingSpotDtin dtin)
        {
            if (string.IsNullOrWhiteSpace(dtin.SpotNumber))
            {
                throw new ArgumentException("El número identificador de la plaza es obligatorio.");
            }

            return await _spotRepository.AddAsync(dtin);
        }

        public async Task<bool> UpdateSpotStatusAsync(int id, SpotStatus status)
        {
            return await _spotRepository.UpdateStatusAsync(id, status);
        }

        public async Task<bool> DeleteSpotAsync(int id)
        {
            return await _spotRepository.DeleteAsync(id);
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Hubs;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class ParkingSpotService : IParkingSpotService
    {
        private readonly IParkingSpotRepository _spotRepository;
        private readonly IHubContext<ParkingHub> _hubContext;

        public ParkingSpotService(
            IParkingSpotRepository spotRepository,
            IHubContext<ParkingHub> hubContext)
        {
            _spotRepository = spotRepository;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<ParkingSpotDto>> GetSpotsByParkingIdAsync(int parkingId)
            => await _spotRepository.GetByParkingIdAsync(parkingId);

        public async Task<ParkingSpotDto?> GetSpotByIdAsync(int id)
            => await _spotRepository.GetByIdAsync(id);

        public async Task<ParkingSpotDto> AddSpotAsync(ParkingSpotDtin dtin)
        {
            if (string.IsNullOrWhiteSpace(dtin.SpotNumber))
                throw new ArgumentException("El número identificador de la plaza es obligatorio.");

            return await _spotRepository.AddAsync(dtin);
        }

        /// <summary>
        /// Actualiza el estado de la plaza en BD y notifica en tiempo real
        /// a todos los clientes suscritos al grupo del parking correspondiente.
        /// </summary>
        public async Task<bool> UpdateSpotStatusAsync(int id, SpotStatus status)
        {
            var updated = await _spotRepository.UpdateStatusAsync(id, status);
            if (!updated) return false;

            // Recuperamos la plaza para conocer su parkingId y número
            var spot = await _spotRepository.GetByIdAsync(id);
            if (spot != null)
            {
                var payload = new SpotStatusChangedPayload
                {
                    SpotId     = spot.Id,
                    ParkingId  = spot.ParkingId,
                    SpotNumber = spot.SpotNumber,
                    Status     = status.ToString(),
                    UpdatedAt  = DateTime.UtcNow
                };

                // Enviamos el evento solo al grupo del parking afectado
                await _hubContext
                    .Clients
                    .Group(ParkingHub.GroupName(spot.ParkingId))
                    .SendAsync("SpotStatusChanged", payload);
            }

            return true;
        }

        public async Task<bool> DeleteSpotAsync(int id)
            => await _spotRepository.DeleteAsync(id);
    }
}
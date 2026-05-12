using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _resRepo;
        private readonly IParkingSpotRepository _spotRepo;
        private readonly ITarifRepository _tarifRepo;
        private readonly ICarRepository _carRepo;

        public ReservationService(IReservationRepository resRepo, IParkingSpotRepository spotRepo, ITarifRepository tarifRepo, ICarRepository carRepo)
        {
            _resRepo = resRepo;
            _spotRepo = spotRepo;
            _tarifRepo = tarifRepo;
            _carRepo = carRepo;
        }

        private async Task<decimal> CalculatePrice(ReservationDtin dtin)
        {
            var spot = await _spotRepo.GetByIdAsync(dtin.ParkingSpotId);
            var car = await _carRepo.GetByIdAsync(dtin.CarId);
            var tarifs = await _tarifRepo.GetByParkingIdAsync(spot!.ParkingId);
            var tarif = tarifs.FirstOrDefault(); 

            decimal pricePerHour = tarif!.PricePerHour;
            if (car!.LargeVehicle) pricePerHour += tarif.LargeVehicleSurcharge;
            if (car!.ElectricVehicle) pricePerHour += tarif.ElectricVehicleSurcharge;

            var duration = dtin.EndTime - dtin.StartTime;
            return (decimal)duration.TotalHours * pricePerHour;
        }

        public async Task<ReservationDto> CreateReservationAsync(ReservationDtin dtin)
        {
            // 1. Validar disponibilidad
            bool isAvailable = await _resRepo.IsSpotAvailableAsync(dtin.ParkingSpotId, dtin.StartTime, dtin.EndTime);
            if (!isAvailable) throw new Exception("Plaza ocupada");

            // 2. Obtener datos para el cálculo
            var spot = await _spotRepo.GetByIdAsync(dtin.ParkingSpotId);
            var car = await _carRepo.GetByIdAsync(dtin.CarId);
            var tarifs = await _tarifRepo.GetByParkingIdAsync(spot!.ParkingId);
            var tarif = tarifs.FirstOrDefault(); 

            // 3. Cálculo de Precio
            decimal pricePerHour = tarif!.PricePerHour;
            if (car!.LargeVehicle) pricePerHour += tarif.LargeVehicleSurcharge;
            if (car!.ElectricVehicle) pricePerHour += tarif.ElectricVehicleSurcharge;

            var duration = dtin.EndTime - dtin.StartTime;
            decimal total = (decimal)duration.TotalHours * pricePerHour;

            // 4. Guardar
            return await _resRepo.CreateAsync(dtin, total);
        }

        public async Task<bool> UpdateReservationAsync(int id, ReservationDtin dtin)
        {
            var existing = await _resRepo.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.StartTime != dtin.StartTime || existing.EndTime != dtin.EndTime)
            {
                bool isAvailable = await _resRepo.IsSpotAvailableAsync(dtin.ParkingSpotId, dtin.StartTime, dtin.EndTime);
                if (!isAvailable) throw new InvalidOperationException("El nuevo horario no está disponible.");
            }

            decimal newTotal = await CalculatePrice(dtin);
            return await _resRepo.UpdateAsync(id, dtin, newTotal);
        }

        public async Task<IEnumerable<ReservationDto>> GetByCompanyAsync(int companyId) => 
            await _resRepo.GetByCompanyIdAsync(companyId);

        public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId) => 
            await _resRepo.GetByUserIdAsync(userId);

        public async Task<ReservationDto?> GetReservationByIdAsync(int id) => 
            await _resRepo.GetByIdAsync(id);

        public async Task<bool> CancelReservationAsync(int id) => 
            await _resRepo.UpdateStatusAsync(id, ReservationStatus.Cancelled);

        public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync() 
        {
            return await _resRepo.GetAllAsync();
        }

    }
}
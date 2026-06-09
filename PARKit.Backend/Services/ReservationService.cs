using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;
using PARKit.Backend.DTOs.PaymentDtin;
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
        private readonly IPaymentRepository _paymentRepo;

        public ReservationService(
            IReservationRepository resRepo,
            IParkingSpotRepository spotRepo,
            ITarifRepository tarifRepo,
            ICarRepository carRepo,
            IPaymentRepository paymentRepo)
        {
            _resRepo = resRepo;
            _spotRepo = spotRepo;
            _tarifRepo = tarifRepo;
            _carRepo = carRepo;
            _paymentRepo = paymentRepo;
        }

        private async Task<decimal> CalculatePriceAsync(ReservationDtin dtin)
        {
            var spot = await _spotRepo.GetByIdAsync(dtin.ParkingSpotId)
                ?? throw new KeyNotFoundException($"No se encontró la plaza con ID {dtin.ParkingSpotId}.");

            var car = await _carRepo.GetByIdAsync(dtin.CarId)
                ?? throw new KeyNotFoundException($"No se encontró el vehículo con ID {dtin.CarId}.");

            var tarifs = await _tarifRepo.GetByParkingIdAsync(spot.ParkingId);
            var tarif = tarifs.FirstOrDefault()
                ?? throw new InvalidOperationException($"El parking no tiene tarifas configuradas.");

            decimal pricePerHour = tarif.PricePerHour;
            if (car.LargeVehicle)    pricePerHour += tarif.LargeVehicleSurcharge;
            if (car.ElectricVehicle) pricePerHour += tarif.ElectricVehicleSurcharge;

            var duration = dtin.EndTime - dtin.StartTime;
            if (duration.TotalHours <= 0)
                throw new ArgumentException("La hora de fin debe ser posterior a la de inicio.");

            // 1. Estancia base
            decimal estanciaBruta = (decimal)duration.TotalHours * pricePerHour;
            
            // 2. NUEVO: Sumar Suplemento de Reserva (si existe)
            decimal suplementoReserva = tarif.ReservationSurcharge;

            // 3. Gastos de gestión PARKit (1.50€)
            return estanciaBruta + suplementoReserva + 1.50m;
        }

        public async Task<ReservationDto> CreateReservationAsync(ReservationDtin dtin)
        {
            bool isAvailable = await _resRepo.IsSpotAvailableAsync(dtin.ParkingSpotId, dtin.StartTime, dtin.EndTime);
            if (!isAvailable)
                throw new InvalidOperationException("La plaza ya está ocupada.");

            decimal total = await CalculatePriceAsync(dtin);
            return await _resRepo.CreateAsync(dtin, total);
        }

        public async Task<bool> CancelReservationAsync(int id)
        {
            // 1. Obtener la reserva y su tarifa
            var res = await _resRepo.GetByIdAsync(id);
            if (res == null || res.Status == ReservationStatus.Cancelled) return false;

            var spot = await _spotRepo.GetByIdAsync(res.ParkingSpotId);
            var tarifs = await _tarifRepo.GetByParkingIdAsync(spot.ParkingId);
            var tarif = tarifs.FirstOrDefault();

            // 2. Cambiar estado de reserva
            var cancelled = await _resRepo.UpdateStatusAsync(id, ReservationStatus.Cancelled);
            if (!cancelled) return false;

            // 3. Lógica de Devolución
            var originalPayment = await _paymentRepo.GetByReservationIdAsync(id);
            if (originalPayment != null)
            {
                decimal feeCancelacion = tarif?.CancellationFee ?? 0;
                // La devolución es el importe original (negativo) + la penalización (que el usuario NO recupera)
                // Ej: Pagó 10€. Penalty 2€. Devolvemos -8€. (El balance global es 2€ pagados).
                decimal importeDevolucion = -(originalPayment.Amount - feeCancelacion);

                if (importeDevolucion < 0)
                {
                    await _paymentRepo.CreateAsync(new PaymentDtin
                    {
                        ReservationId = id,
                        Amount = importeDevolucion,
                        Currency = originalPayment.Currency,
                        Status = PaymentStatus.Paid, // Marcamos como pagado el refund
                        ExternalTransactionId = $"REFUND_{id}_{DateTime.Now.Ticks}"
                    });
                }
            }

            return true;
        }

        public async Task<bool> UpdateReservationAsync(int id, ReservationDtin dtin)
        {
            decimal newTotal = await CalculatePriceAsync(dtin);
            return await _resRepo.UpdateAsync(id, dtin, newTotal);
        }

        public async Task<IEnumerable<ReservationDto>> GetByCompanyAsync(int companyId) => await _resRepo.GetByCompanyIdAsync(companyId);
        public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId) => await _resRepo.GetByUserIdAsync(userId);
        public async Task<ReservationDto?> GetReservationByIdAsync(int id) => await _resRepo.GetByIdAsync(id);
        public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync() => await _resRepo.GetAllAsync();
    }
}

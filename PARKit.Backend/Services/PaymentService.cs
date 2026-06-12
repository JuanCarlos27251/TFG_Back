using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentDtin;
using PARKit.Backend.DTOs.PaymentMethodDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;
using Stripe;
using Microsoft.AspNetCore.SignalR;
using PARKit.Backend.Hubs;

namespace PARKit.Backend.Services
{
    public class PaymentService : IPaymentService, IPaymentMethodService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPaymentMethodRepository _methodRepo;
        private readonly IReservationRepository _resRepo;
        private readonly IParkingRepository _parkingRepo;
        private readonly IParkingSpotRepository _spotRepo;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IPaymentMethodRepository methodRepo,
            IReservationRepository resRepo,
            IParkingRepository parkingRepo,
            IParkingSpotRepository spotRepo,
            IHubContext<ParkingHub> hubContext,
            IConfiguration configuration)
        {
              _paymentRepo = paymentRepo;
            _methodRepo = methodRepo;
            _resRepo = resRepo;
            _parkingRepo = parkingRepo;
            _spotRepo = spotRepo;
            _hubContext = hubContext;
            _configuration = configuration;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        // --- Métodos de Pago ---
        public async Task<IEnumerable<PaymentMethodDto>> GetUserMethodsAsync(int userId)
            => await _methodRepo.GetByUserIdAsync(userId);

        public async Task<PaymentMethodDto> AddMethodAsync(PaymentMethodDtin dtin)
            => await _methodRepo.AddAsync(dtin);

        public async Task<bool> DeleteMethodAsync(int id)
            => await _methodRepo.DeleteAsync(id);

        // --- Pagos ---
        public async Task<PaymentDto?> GetByReservationAsync(int reservationId)
            => await _paymentRepo.GetByReservationIdAsync(reservationId);

        public async Task<PaymentDto> CreatePaymentAsync(PaymentDtin dtin)
        {
            // 1. Creamos el PaymentIntent en Stripe
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dtin.Amount * 100), // Stripe trabaja en céntimos
                Currency = dtin.Currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "ReservationId", dtin.ReservationId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            // 2. Guardamos el pago en BD con el ClientSecret que devuelve Stripe
            var paymentDtin = new PaymentDtin
            {
                ReservationId = dtin.ReservationId,
                Amount = dtin.Amount,
                Currency = dtin.Currency,
                Status = PaymentStatus.Pending,
                ExternalTransactionId = intent.Id
            };

            // Necesitamos guardar el ClientSecret, lo hacemos via el repositorio
            var payment = await _paymentRepo.CreateWithSecretAsync(paymentDtin, intent.ClientSecret);

            return payment;
        }

        public async Task<bool> ConfirmPaymentAsync(int paymentId, string externalTransactionId)
        {
            var updated = await _paymentRepo.UpdateStatusAsync(
                paymentId, PaymentStatus.Paid, externalTransactionId);

            if (!updated) return false;

            // Buscamos la reserva asociada y la confirmamos
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment != null)
            {
                await _resRepo.UpdateStatusAsync(payment.ReservationId, ReservationStatus.Confirmed);
                try 
                {
                    var reservation = await _resRepo.GetByIdAsync(payment.ReservationId);
                    if (reservation != null) 
                    {
                        var spot = await _spotRepo.GetByIdAsync(reservation.ParkingSpotId);
                        if (spot != null)
                        {
                            var parking = await _parkingRepo.GetByIdAsync(spot.ParkingId);
                            if (parking != null)
                            {
                                int newCount = parking.AvailableSpots;
                                // Envía el evento UpdateSpots a todo el mundo conectado
                                await _hubContext.Clients.All.SendAsync("UpdateSpots", parking.Id, newCount);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    // Evitamos que un fallo de red o SignalR bloquee el pago del cliente
                    Console.WriteLine("Error enviando actualización por SignalR: " + ex.Message);
                }
            }

            return true;
        }

    }
}
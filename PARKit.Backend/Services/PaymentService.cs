using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentDtin;
using PARKit.Backend.DTOs.PaymentMethodDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class PaymentService : IPaymentService, IPaymentMethodService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPaymentMethodRepository _methodRepo;
        private readonly IReservationRepository _resRepo;

        public PaymentService(IPaymentRepository paymentRepo, IPaymentMethodRepository methodRepo,IReservationRepository resRepo)
        {
            _paymentRepo = paymentRepo;
            _methodRepo = methodRepo;
            _resRepo = resRepo;
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
            var payment = await _paymentRepo.CreateAsync(dtin);
            
            if (dtin.Status == PaymentStatus.Paid)
            {
                await _resRepo.UpdateStatusAsync(dtin.ReservationId, ReservationStatus.Confirmed);
            }
            
            return payment;
        }

        public async Task<bool> ConfirmPaymentAsync(int paymentId, string externalTransactionId)
        {
            return await _paymentRepo.UpdateStatusAsync(paymentId, PaymentStatus.Paid, externalTransactionId);
        }

        
    }
}
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetByReservationAsync(int reservationId);
        Task<PaymentDto> CreatePaymentAsync(PaymentDtin dtin);
        Task<bool> ConfirmPaymentAsync(int paymentId, string externalTransactionId);
    }
}
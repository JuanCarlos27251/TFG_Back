using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentDtin;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Repositories
{
    public interface IPaymentRepository
    {
        Task<PaymentDto?> GetByReservationIdAsync(int reservationId);
        Task<PaymentDto> CreateAsync(PaymentDtin dtin);
        Task<bool> UpdateStatusAsync(int id, PaymentStatus status, string? externalId = null);
        Task<PaymentDto> CreateWithSecretAsync(PaymentDtin dtin, string clientSecret);
        Task<PaymentDto?> GetByIdAsync(int id);
    }
}
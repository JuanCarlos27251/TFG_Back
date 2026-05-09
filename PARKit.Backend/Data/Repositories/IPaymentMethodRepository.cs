using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentMethodDtin;

namespace PARKit.Backend.Repositories
{
    public interface IPaymentMethodRepository
    {
        Task<IEnumerable<PaymentMethodDto>> GetByUserIdAsync(int userId);
        Task<PaymentMethodDto?> GetByIdAsync(int id);
        Task<PaymentMethodDto> AddAsync(PaymentMethodDtin dtin);
        Task<bool> DeleteAsync(int id);
    }
}
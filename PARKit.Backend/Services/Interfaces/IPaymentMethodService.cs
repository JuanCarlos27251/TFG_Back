using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentMethodDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<IEnumerable<PaymentMethodDto>> GetUserMethodsAsync(int userId);
        Task<PaymentMethodDto> AddMethodAsync(PaymentMethodDtin dtin);
        Task<bool> DeleteMethodAsync(int id);
    }
}
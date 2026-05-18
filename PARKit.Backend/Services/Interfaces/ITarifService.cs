using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.TarifDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface ITarifService
    {
        Task<IEnumerable<TarifDto>> GetTarifsByParkingIdAsync(int parkingId);
        Task<TarifDto?> GetTarifByIdAsync(int id);
        Task<TarifDto> AddTarifAsync(TarifDtin dtin);
        Task<bool> UpdateTarifAsync(int id, TarifDtin dtin);
        Task<bool> DeleteTarifAsync(int id);
    }
}
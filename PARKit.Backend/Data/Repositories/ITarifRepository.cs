using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.TarifDtin;

namespace PARKit.Backend.Repositories
{
    public interface ITarifRepository
    {
        Task<IEnumerable<TarifDto>> GetByParkingIdAsync(int parkingId);
        Task<TarifDto?> GetByIdAsync(int id);
        Task<TarifDto> AddAsync(TarifDtin dtin);
        Task<bool> UpdateAsync(int id, TarifDtin dtin);
        Task<bool> DeleteAsync(int id);
    }
}
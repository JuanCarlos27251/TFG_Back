using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CarDtin;


namespace PARKit.Backend.Repositories
{
    public interface ICarRepository
    {
        Task<List<CarDto>> GetCarsByUserIdAsync(int userId);
        Task<CarDto?> GetByIdAsync(int id);
        Task<CarDto> AddAsync(int userId, CarDtin carDtin);
        Task<bool> UpdateAsync(int id, int userId, CarDtin carDtin);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
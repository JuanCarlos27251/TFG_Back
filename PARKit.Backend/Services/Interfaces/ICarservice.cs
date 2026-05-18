using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CarDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface ICarService
    {
        Task<List<CarDto>> GetCarsByUserIdAsync(int userId);
        Task<CarDto?> GetCarByIdAsync(int id);
        Task<CarDto> AddCarAsync(int userId, CarDtin carDtin);
        Task<bool> UpdateCarAsync(int id, int userId, CarDtin carDtin);
        Task<bool> DeleteCarAsync(int id, int userId);
    }
}
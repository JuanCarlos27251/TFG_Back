using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CarDtin;

using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;

        public CarService (ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        public async Task<List<CarDto>> GetCarsByUserIdAsync(int userId)
        {
            return await _carRepository.GetCarsByUserIdAsync(userId);
        }

        public async Task<CarDto?> GetCarByIdAsync(int id)
        {
            return await _carRepository.GetByIdAsync(id);
        }

        public async Task<CarDto> AddCarAsync(int userId, CarDtin carDtin)
        {
            if (string.IsNullOrWhiteSpace(carDtin.Name))
                throw new ArgumentException("El nombre del vehículo es obligatorio.");  
            if (string.IsNullOrWhiteSpace(carDtin.Matricule))
                throw new ArgumentException("La matrícula es obligatoria.");
            return await _carRepository.AddAsync(userId, carDtin);
        }

        public async Task<bool> UpdateCarAsync(int id, int userId, CarDtin carDtin)
        {
            if (string.IsNullOrWhiteSpace(carDtin.Name))
                throw new ArgumentException("El nombre del vehículo no puede estar vacío.");  
            if (string.IsNullOrWhiteSpace(carDtin.Matricule))
                throw new ArgumentException("La matrícula no puede estar vacía.");
            return await _carRepository.UpdateAsync(id, userId, carDtin);
        }

        public async Task<bool> DeleteCarAsync(int id, int userId)
        {
            return await _carRepository.DeleteAsync(id, userId);
        }


    }
}
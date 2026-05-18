using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CarDtin;
using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly AppDbContext _context;

        public CarRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CarDto>> GetCarsByUserIdAsync(int userId)
        {
            return await _context.Cars
                .Where(c => c.UserId == userId)
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Matricule = c.Matricule,
                    LargeVehicle = c.LargeVehicle,
                    ElectricVehicle = c.ElectricVehicle,
                    UserId = c.UserId
                }).ToListAsync();
        }

        public async Task<CarDto> AddAsync(int userId, CarDtin carDtin)
        {
            var car = new Car
            {
                UserId = userId,
                Matricule = carDtin.Matricule,
                LargeVehicle = carDtin.LargeVehicle,
                ElectricVehicle = carDtin.ElectricVehicle
            };

            await _context.Cars.AddAsync(car);
            await _context.SaveChangesAsync();

            return new CarDto { Id = car.Id, Matricule = car.Matricule, UserId = userId };
        }
        public async Task<CarDto?> GetByIdAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null) return null;

            return new CarDto
            {
                Id = car.Id,
                Matricule = car.Matricule,
                LargeVehicle = car.LargeVehicle,
                ElectricVehicle = car.ElectricVehicle,
                UserId = car.UserId
            };
        }

        public async Task<bool> UpdateAsync(int id, int userId, CarDtin carDtin)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (car == null) return false;

            // Actualizamos los campos
            car.Matricule = carDtin.Matricule;

            car.LargeVehicle = carDtin.LargeVehicle;
            car.ElectricVehicle = carDtin.ElectricVehicle;

            _context.Cars.Update(car);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            // Importante: verificamos que el coche pertenezca al usuario que intenta borrarlo
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (car == null) return false;

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
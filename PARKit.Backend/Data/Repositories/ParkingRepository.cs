using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
   public class ParkingRepository : IParkingRepository
    {
        private readonly AppDbContext _context;

        public ParkingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingDto>> GetAllAsync()
        {
            return await _context.Parkings
                .Select(p => new ParkingDto
                {
                    Id = p.Id,
                    CompanyId = p.CompanyId,
                    Name = p.Name,
                    Description = p.Description,
                    Address = p.Address,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Type = p.Type,
                    IsActive = p.IsActive,
                    ImageUrl = p.ImageUrl,
                    GeometryData = p.GeometryData,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
        }

        public async Task<ParkingDto?> GetByIdAsync(int id)
        {
            var p = await _context.Parkings.FindAsync(id);
            if (p == null) return null;

            return new ParkingDto
            {
                Id = p.Id, Name = p.Name, Address = p.Address, 
                Latitude = p.Latitude, Longitude = p.Longitude,
                Type = p.Type, IsActive = p.IsActive, CompanyId = p.CompanyId
            };
        }

        public async Task<IEnumerable<ParkingDto>> GetByCompanyIdAsync(int companyId)
        {
            return await _context.Parkings
                .Where(p => p.CompanyId == companyId)
                .Select(p => new ParkingDto
                {
                    Id = p.Id,
                    CompanyId = p.CompanyId,
                    Name = p.Name,
                    Description = p.Description,
                    Address = p.Address,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Type = p.Type,
                    IsActive = p.IsActive,
                    ImageUrl = p.ImageUrl,
                    GeometryData = p.GeometryData,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
        }

        public async Task<ParkingDto> AddAsync(ParkingDtin dtin)
        {
            var parking = new Parkings // Tu modelo se llama Parkings
            {
                CompanyId = dtin.CompanyId,
                Name = dtin.Name,
                Description = dtin.Description,
                Address = dtin.Address,
                Latitude = dtin.Latitude,
                Longitude = dtin.Longitude,
                Type = dtin.Type,
                IsActive = dtin.IsActive,
                ImageUrl = dtin.ImageUrl,
                GeometryData = dtin.GeometryData
            };

            await _context.Parkings.AddAsync(parking);
            await _context.SaveChangesAsync();

            return new ParkingDto { Id = parking.Id, Name = parking.Name };
        }

        public async Task<bool> UpdateAsync(int id, ParkingDtin dtin)
        {
            var p = await _context.Parkings.FindAsync(id);
            if (p == null) return false;

            p.Name = dtin.Name;
            p.Description = dtin.Description;
            p.Address = dtin.Address;
            p.Latitude = dtin.Latitude;
            p.Longitude = dtin.Longitude;
            p.Type = dtin.Type;
            p.IsActive = dtin.IsActive;
            p.ImageUrl = dtin.ImageUrl;
            p.GeometryData = dtin.GeometryData;

            _context.Parkings.Update(p);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var p = await _context.Parkings.FindAsync(id);
            if (p == null) return false;

            _context.Parkings.Remove(p);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
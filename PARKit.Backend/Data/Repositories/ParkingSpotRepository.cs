using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
    public class ParkingSpotRepository : IParkingSpotRepository
    {
        private readonly AppDbContext _context;

        public ParkingSpotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingSpotDto>> GetByParkingIdAsync(int parkingId)
        {
            return await _context.ParkingSpots
                .Where(s => s.ParkingId == parkingId)
                .Select(s => new ParkingSpotDto
                {
                    Id = s.Id,
                    ParkingId = s.ParkingId,
                    SpotNumber = s.SpotNumber,
                    Status = s.Status,
                    Type = s.Type,
                    LastUpdated = s.LastUpdated,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                }).ToListAsync();
        }

        public async Task<ParkingSpotDto> AddAsync(ParkingSpotDtin dtin)
        {
            var spot = new ParkingSpot
            {
                ParkingId = dtin.ParkingId,
                SpotNumber = dtin.SpotNumber,
                Status = dtin.Status,
                Type = dtin.Type,
                Latitude = dtin.Latitude,
                Longitude = dtin.Longitude,
                LastUpdated = DateTime.UtcNow
            };

            await _context.ParkingSpots.AddAsync(spot);
            await _context.SaveChangesAsync();

            return new ParkingSpotDto { Id = spot.Id, SpotNumber = spot.SpotNumber };
        }

        public async Task<bool> UpdateStatusAsync(int id, SpotStatus status)
        {
            var spot = await _context.ParkingSpots.FindAsync(id);
            if (spot == null) return false;

            spot.Status = status;
            spot.LastUpdated = DateTime.UtcNow;

            _context.ParkingSpots.Update(spot);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ParkingSpotDto?> GetByIdAsync(int id)
        {
            var s = await _context.ParkingSpots.FindAsync(id);
            if (s == null) return null;

            return new ParkingSpotDto
            {
                Id = s.Id,
                ParkingId = s.ParkingId,
                SpotNumber = s.SpotNumber,
                Status = s.Status,
                Type = s.Type
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var spot = await _context.ParkingSpots.FindAsync(id);
            if (spot == null) return false;

            _context.ParkingSpots.Remove(spot);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

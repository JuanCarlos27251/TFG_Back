using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace PARKit.Backend.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReservationDto>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.ParkingSpot)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ParkingSpotId = r.ParkingSpotId,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status
                }).ToListAsync();
        }

        public async Task<bool> IsSpotAvailableAsync(int spotId, DateTime start, DateTime end)
        {
            return !await _context.Reservations.AnyAsync(r => 
                r.ParkingSpotId == spotId && 
                r.Status != ReservationStatus.Cancelled &&
                ((start >= r.StartTime && start < r.EndTime) || 
                 (end > r.StartTime && end <= r.EndTime)));
        }

        public async Task<ReservationDto> CreateAsync(ReservationDtin dtin)
        {
            var reservation = new Reservation
            {
                UserId = dtin.UserId,
                ParkingSpotId = dtin.ParkingSpotId,
                StartTime = dtin.StartTime,
                EndTime = dtin.EndTime,
                Status = dtin.Status
            };

            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            return new ReservationDto { Id = reservation.Id, Status = reservation.Status };
        }

        public async Task<bool> UpdateStatusAsync(int id, ReservationStatus status)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return false;

            res.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return null;
            return new ReservationDto { Id = r.Id, UserId = r.UserId, Status = r.Status };
        }
    }
}
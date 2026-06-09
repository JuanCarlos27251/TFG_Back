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
                .ThenInclude(s => s.Parking)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationDto
                {
                     Id = r.Id,
                    UserId = r.UserId,
                    ParkingSpotId = r.ParkingSpotId,
                    SpotNumber = r.ParkingSpot != null ? r.ParkingSpot.SpotNumber : "N/A",
                    ParkingName = r.ParkingSpot != null && r.ParkingSpot.Parking != null ? r.ParkingSpot.Parking.Name : "Parking Municipal (Externa)",
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status,
                    TotalAmount = r.TotalAmount 
                }).ToListAsync();
        }
 
        public async Task<bool> IsSpotAvailableAsync(int spotId, DateTime start, DateTime end)
        {
            return !await _context.Reservations.AnyAsync(r =>
                r.ParkingSpotId == spotId &&
                r.Status != ReservationStatus.Cancelled &&
                start < r.EndTime && 
                end > r.StartTime);
        }
 
        public async Task<ReservationDto> CreateAsync(ReservationDtin dtin, decimal totalAmount)
        {
            var reservation = new Reservation
            {
                UserId = dtin.UserId,
                ParkingSpotId = dtin.ParkingSpotId,
                CarId = dtin.CarId,
                StartTime = dtin.StartTime,
                EndTime = dtin.EndTime,
                Status = dtin.Status,
                TotalAmount = totalAmount
            };
 
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();
 
            return new ReservationDto
            {
                Id = reservation.Id,
                Status = reservation.Status,
                TotalAmount = reservation.TotalAmount
            };
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
            var r = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (r == null) return null;

            return new ReservationDto
            {
                Id            = r.Id,
                UserId        = r.UserId,
                ParkingSpotId = r.ParkingSpotId,
                SpotNumber    = r.ParkingSpot?.SpotNumber ?? "N/A",
                StartTime     = r.StartTime,
                EndTime       = r.EndTime,
                Status        = r.Status,
                TotalAmount   = r.TotalAmount
            };
        }
 
        public async Task<IEnumerable<ReservationDto>> GetByCompanyIdAsync(int companyId)
        {
            return await _context.Reservations
                .Include(r => r.ParkingSpot)
                .Where(r => r.ParkingSpot != null && _context.Parkings
                    .Any(p => p.Id == r.ParkingSpot.ParkingId && p.CompanyId == companyId))
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ParkingSpotId = r.ParkingSpotId,
                    SpotNumber = r.ParkingSpot != null ? r.ParkingSpot.SpotNumber : "N/A",
                    ParkingName = "Parking ID: " + (r.ParkingSpot != null ? r.ParkingSpot.ParkingId : 0),
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status,
                    TotalAmount = r.TotalAmount
                })
                .ToListAsync();
        }
 
        public async Task<IEnumerable<ReservationDto>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.ParkingSpot)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ParkingSpotId = r.ParkingSpotId,
                    SpotNumber = r.ParkingSpot != null ? r.ParkingSpot.SpotNumber : "N/A",
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status,
                    TotalAmount = r.TotalAmount
                })
                .ToListAsync();
        }
 
        public async Task<bool> UpdateAsync(int id, ReservationDtin dtin, decimal totalAmount)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return false;
 
            res.StartTime = dtin.StartTime;
            res.EndTime = dtin.EndTime;
            res.TotalAmount = totalAmount;
            res.Status = dtin.Status;
 
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
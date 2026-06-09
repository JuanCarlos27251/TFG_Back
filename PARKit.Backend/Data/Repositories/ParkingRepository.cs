using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Enums;
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
                .Include(p => p.ParkingSpots)
                .Include(p => p.Tarifs)
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
                    CreatedAt = p.CreatedAt,
                    AvailableSpots = p.ParkingSpots.Count(s => s.Status == SpotStatus.Free),
                    Spots = p.ParkingSpots.Select(s => new ParkingSpotDto
                    {
                        Id = s.Id,
                        ParkingId = s.ParkingId,
                        SpotNumber = s.SpotNumber,
                        Status = s.Status,
                        Type = s.Type,
                        LastUpdated = s.LastUpdated,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude
                    }).ToList(),
                    Tarifs = p.Tarifs.Select(t => new TarifDto
                    {
                        Id = t.Id,
                        ParkingId = t.ParkingId,
                        NameTarif = t.NameTarif,
                        PricePerHour = t.PricePerHour,
                        LargeVehicleSurcharge = t.LargeVehicleSurcharge,
                        ElectricVehicleSurcharge = t.ElectricVehicleSurcharge,
                        ReservationSurcharge = t.ReservationSurcharge,
                        CancellationFee = t.CancellationFee,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        IsHoliday = t.IsHoliday,
                        StarTime = t.StarTime,
                        EndTime = t.EndTime
                    }).ToList()
                }).ToListAsync();
        }
 
        public async Task<ParkingDto?> GetByIdAsync(int id)
        {
            var p = await _context.Parkings
                .Include(p => p.ParkingSpots)
                .Include(p => p.Tarifs)
                .FirstOrDefaultAsync(p => p.Id == id);
 
            if (p == null) return null;
 
            return new ParkingDto
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
                CreatedAt = p.CreatedAt,
                AvailableSpots = p.ParkingSpots.Count(s => s.Status == SpotStatus.Free),
                Spots = p.ParkingSpots.Select(s => new ParkingSpotDto
                {
                    Id = s.Id,
                    ParkingId = s.ParkingId,
                    SpotNumber = s.SpotNumber,
                    Status = s.Status,
                    Type = s.Type,
                    LastUpdated = s.LastUpdated,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                }).ToList(),
                Tarifs = p.Tarifs.Select(t => new TarifDto
                {
                    Id = t.Id,
                    ParkingId = t.ParkingId,
                    NameTarif = t.NameTarif,
                    PricePerHour = t.PricePerHour,
                    LargeVehicleSurcharge = t.LargeVehicleSurcharge,
                    ElectricVehicleSurcharge = t.ElectricVehicleSurcharge,
                    ReservationSurcharge = t.ReservationSurcharge,
                    CancellationFee = t.CancellationFee,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsHoliday = t.IsHoliday,
                    StarTime = t.StarTime,
                    EndTime = t.EndTime
                }).ToList()
            };
        }
 
        public async Task<IEnumerable<ParkingDto>> GetByCompanyIdAsync(int companyId)
        {
            return await _context.Parkings
                .Include(p => p.ParkingSpots)
                .Include(p => p.Tarifs)
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
                    CreatedAt = p.CreatedAt,
                    AvailableSpots = p.ParkingSpots.Count(s => s.Status == SpotStatus.Free),
                    Spots = p.ParkingSpots.Select(s => new ParkingSpotDto
                    {
                        Id = s.Id,
                        ParkingId = s.ParkingId,
                        SpotNumber = s.SpotNumber,
                        Status = s.Status,
                        Type = s.Type,
                        LastUpdated = s.LastUpdated,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude
                    }).ToList(),
                    Tarifs = p.Tarifs.Select(t => new TarifDto
                    {
                        Id = t.Id,
                        ParkingId = t.ParkingId,
                        NameTarif = t.NameTarif,
                        PricePerHour = t.PricePerHour,
                        LargeVehicleSurcharge = t.LargeVehicleSurcharge,
                        ElectricVehicleSurcharge = t.ElectricVehicleSurcharge,
                        ReservationSurcharge = t.ReservationSurcharge,
                        CancellationFee = t.CancellationFee,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        IsHoliday = t.IsHoliday,
                        StarTime = t.StarTime,
                        EndTime = t.EndTime
                    }).ToList()
                }).ToListAsync();
        }
 
        public async Task<ParkingDto> AddAsync(ParkingDtin dtin)
        {
            var parking = new Parkings
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
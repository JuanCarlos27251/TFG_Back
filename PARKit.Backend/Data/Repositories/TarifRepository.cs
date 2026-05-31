using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.TarifDtin;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
    public class TarifRepository : ITarifRepository
    {
        private readonly AppDbContext _context;

        public TarifRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TarifDto>> GetByParkingIdAsync(int parkingId)
        {
            return await _context.Tarifs
                .Where(t => t.ParkingId == parkingId)
                .Select(t => new TarifDto
                {
                    Id = t.Id,
                    ParkingId = t.ParkingId,
                    PricePerHour = t.PricePerHour,
                    NameTarif = t.NameTarif,
                    LargeVehicleSurcharge    = t.LargeVehicleSurcharge,    
                    ElectricVehicleSurcharge = t.ElectricVehicleSurcharge,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsHoliday = t.IsHoliday,
                    StarTime = t.StarTime,
                    EndTime = t.EndTime
                }).ToListAsync();
        }

        public async Task<TarifDto?> GetByIdAsync(int id)
        {
            var t = await _context.Tarifs.FindAsync(id);
            if (t == null) return null;

            return new TarifDto
            {
                Id = t.Id,
                ParkingId = t.ParkingId,
                PricePerHour = t.PricePerHour,
                NameTarif = t.NameTarif,
                IsHoliday = t.IsHoliday
            };
        }

        public async Task<TarifDto> AddAsync(TarifDtin dtin)
        {
            var tarif = new Tarif
            {
                ParkingId = dtin.ParkingId,
                PricePerHour = dtin.PricePerHour,
                NameTarif = dtin.NameTarif,
                StartDate = dtin.StartDate,
                EndDate = dtin.EndDate,
                IsHoliday = dtin.IsHoliday,
                StarTime = dtin.StarTime,
                EndTime = dtin.EndTime
            };

            await _context.Tarifs.AddAsync(tarif);
            await _context.SaveChangesAsync();

            return new TarifDto { Id = tarif.Id, NameTarif = tarif.NameTarif, PricePerHour = tarif.PricePerHour };
        }

        public async Task<bool> UpdateAsync(int id, TarifDtin dtin)
        {
            var t = await _context.Tarifs.FindAsync(id);
            if (t == null) return false;

            t.PricePerHour = dtin.PricePerHour;
            t.NameTarif = dtin.NameTarif;
            t.StartDate = dtin.StartDate;
            t.EndDate = dtin.EndDate;
            t.IsHoliday = dtin.IsHoliday;
            t.StarTime = dtin.StarTime;
            t.EndTime = dtin.EndTime;

            _context.Tarifs.Update(t);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var t = await _context.Tarifs.FindAsync(id);
            if (t == null) return false;

            _context.Tarifs.Remove(t);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
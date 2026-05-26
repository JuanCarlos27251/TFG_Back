using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.Enums;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Para cada parking de la empresa calcula cuántas plazas hay en total,
        /// cuántas están ocupadas/reservadas y la tasa resultante.
        /// </summary>
        public async Task<IEnumerable<OccupancyDto>> GetOccupancyByCompanyAsync(int companyId)
        {
            return await _context.Parkings
                .Where(p => p.CompanyId == companyId && p.IsActive)
                .Select(p => new OccupancyDto
                {
                    ParkingId    = p.Id,
                    ParkingName  = p.Name,
                    TotalSpots   = p.ParkingSpots.Count,
                    OccupiedSpots = p.ParkingSpots
                        .Count(s => s.Status == SpotStatus.Occupied || s.Status == SpotStatus.Reserved),
                    OccupancyRate = p.ParkingSpots.Count == 0
                        ? 0.0
                        : (double)p.ParkingSpots
                            .Count(s => s.Status == SpotStatus.Occupied || s.Status == SpotStatus.Reserved)
                          / p.ParkingSpots.Count
                })
                .ToListAsync();
        }

        /// <summary>
        /// Agrupa los pagos confirmados por año/mes y suma los ingresos.
        /// Solo se tienen en cuenta los parkings de la empresa.
        /// </summary>
        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int companyId, int months = 6)
        {
            var since = DateTime.UtcNow.AddMonths(-months);

            // Obtenemos los IDs de plazas que pertenecen a la empresa
            var spotIds = await _context.ParkingSpots
                .Where(s => _context.Parkings
                    .Any(p => p.Id == s.ParkingId && p.CompanyId == companyId))
                .Select(s => s.Id)
                .ToListAsync();

            var result = await _context.Reservations
                .Where(r => spotIds.Contains(r.ParkingSpotId)
                         && r.Status != ReservationStatus.Cancelled
                         && r.StartTime >= since)
                .GroupBy(r => new { r.StartTime.Year, r.StartTime.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year               = g.Key.Year,
                    Month              = g.Key.Month,
                    TotalRevenue       = g.Sum(r => r.TotalAmount),
                    TotalReservations  = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Agrupa el inicio de las reservas por hora del día (0–23) y cuenta cuántas hay en cada franja.
        /// Útil para identificar horas pico.
        /// </summary>
        public async Task<IEnumerable<PeakHourDto>> GetPeakHoursAsync(int companyId)
        {
            var spotIds = await _context.ParkingSpots
                .Where(s => _context.Parkings
                    .Any(p => p.Id == s.ParkingId && p.CompanyId == companyId))
                .Select(s => s.Id)
                .ToListAsync();

            return await _context.Reservations
                .Where(r => spotIds.Contains(r.ParkingSpotId)
                         && r.Status != ReservationStatus.Cancelled)
                .GroupBy(r => r.StartTime.Hour)
                .Select(g => new PeakHourDto
                {
                    Hour             = g.Key,
                    ReservationCount = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();
        }

        /// <summary>
        /// Cuenta reservas confirmadas separadas por tipo de vehículo (estándar, grande, eléctrico).
        /// </summary>
        public async Task<VehicleTypeStatsDto> GetVehicleTypeStatsAsync(int companyId)
        {
            var spotIds = await _context.ParkingSpots
                .Where(s => _context.Parkings
                    .Any(p => p.Id == s.ParkingId && p.CompanyId == companyId))
                .Select(s => s.Id)
                .ToListAsync();

            var reservations = await _context.Reservations
                .Include(r => r.Car)
                .Where(r => spotIds.Contains(r.ParkingSpotId)
                         && r.Status != ReservationStatus.Cancelled
                         && r.Car != null)
                .Select(r => new { r.Car!.LargeVehicle, r.Car.ElectricVehicle })
                .ToListAsync();

            return new VehicleTypeStatsDto
            {
                ElectricCount = reservations.Count(r => r.ElectricVehicle),
                LargeCount    = reservations.Count(r => r.LargeVehicle && !r.ElectricVehicle),
                StandardCount = reservations.Count(r => !r.LargeVehicle && !r.ElectricVehicle)
            };
        }
    }
}
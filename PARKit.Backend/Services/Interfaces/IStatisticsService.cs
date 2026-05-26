namespace PARKit.Backend.Services.Interfaces
{
    public class OccupancyDto
    {
        public int ParkingId { get; set; }
        public string ParkingName { get; set; } = string.Empty;
        public int TotalSpots { get; set; }
        public int OccupiedSpots { get; set; }
        public double OccupancyRate { get; set; }  // 0.0 – 1.0
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalReservations { get; set; }
    }

    public class PeakHourDto
    {
        public int Hour { get; set; }           // 0–23
        public int ReservationCount { get; set; }
    }

    public class VehicleTypeStatsDto
    {
        public int StandardCount { get; set; }
        public int LargeCount { get; set; }
        public int ElectricCount { get; set; }
    }

    public interface IStatisticsService
    {
        /// <summary>Tasa de ocupación actual de cada parking de la empresa.</summary>
        Task<IEnumerable<OccupancyDto>> GetOccupancyByCompanyAsync(int companyId);

        /// <summary>Ingresos agrupados por mes para los últimos N meses.</summary>
        Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int companyId, int months = 6);

        /// <summary>Horas del día con más reservas iniciadas (para detectar picos).</summary>
        Task<IEnumerable<PeakHourDto>> GetPeakHoursAsync(int companyId);

        /// <summary>Conteo de reservas por tipo de vehículo.</summary>
        Task<VehicleTypeStatsDto> GetVehicleTypeStatsAsync(int companyId);
    }
}
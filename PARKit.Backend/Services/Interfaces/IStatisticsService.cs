using PARKit.Backend.DTOs;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<OccupancyDto>> GetOccupancyByCompanyAsync(int companyId);
        Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int companyId, int months = 6);
        Task<IEnumerable<PeakHourDto>> GetPeakHoursAsync(int companyId);
        Task<VehicleTypeStatsDto> GetVehicleTypeStatsAsync(int companyId);
    }
}
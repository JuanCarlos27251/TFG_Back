using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statsService;

        public StatisticsController(IStatisticsService statsService)
        {
            _statsService = statsService;
        }

        /// <summary>
        /// Tasa de ocupación actual de cada parking de la empresa.
        /// GET /api/Statistics/company/{companyId}/occupancy
        /// </summary>
        [HttpGet("company/{companyId}/occupancy")]
        public async Task<IActionResult> GetOccupancy(int companyId)
        {
            var result = await _statsService.GetOccupancyByCompanyAsync(companyId);
            return Ok(result);
        }

        /// <summary>
        /// Ingresos mensuales de los últimos N meses (por defecto 6).
        /// GET /api/Statistics/company/{companyId}/revenue?months=6
        /// </summary>
        [HttpGet("company/{companyId}/revenue")]
        public async Task<IActionResult> GetMonthlyRevenue(int companyId, [FromQuery] int months = 6)
        {
            if (months < 1 || months > 24)
                return BadRequest(new { message = "El parámetro 'months' debe estar entre 1 y 24." });

            var result = await _statsService.GetMonthlyRevenueAsync(companyId, months);
            return Ok(result);
        }

        /// <summary>
        /// Horas pico de reservas (0–23).
        /// GET /api/Statistics/company/{companyId}/peak-hours
        /// </summary>
        [HttpGet("company/{companyId}/peak-hours")]
        public async Task<IActionResult> GetPeakHours(int companyId)
        {
            var result = await _statsService.GetPeakHoursAsync(companyId);
            return Ok(result);
        }

        /// <summary>
        /// Distribución de reservas por tipo de vehículo.
        /// GET /api/Statistics/company/{companyId}/vehicle-types
        /// </summary>
        [HttpGet("company/{companyId}/vehicle-types")]
        public async Task<IActionResult> GetVehicleTypes(int companyId)
        {
            var result = await _statsService.GetVehicleTypeStatsAsync(companyId);
            return Ok(result);
        }
    }
}
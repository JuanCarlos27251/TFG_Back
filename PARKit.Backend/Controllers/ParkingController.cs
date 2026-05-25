using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.Enums;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        //private readonly IZaragozaDataService _zaragozaService;

        public ParkingController(IParkingService parkingService)
        {
           _parkingService = parkingService; 
        }

       [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingDto>>> GetAll(
            [FromQuery] ParkingType? type = null,
            [FromQuery] bool? onlyAvailable = null)
        {
            var parkings = await _parkingService.GetAllParkingsAsync(type, onlyAvailable);
            return Ok(parkings);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingDto>> GetById(int id)
        {
            var parking = await _parkingService.GetParkingByIdAsync(id);
            if (parking == null) return NotFound(new { message = "Parking no encontrado" });
            return Ok(parking);
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpGet("manager/{managerId}")]
        public async Task<ActionResult<IEnumerable<ParkingDto>>> GetByManager(int managerId)
        {
            var parkings = await _parkingService.GetByManagerIdAsync(managerId);
            return Ok(parkings);
        }
    }
}
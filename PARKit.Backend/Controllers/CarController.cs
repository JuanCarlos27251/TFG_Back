using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CarDtin;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
   public class CarController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet("MyCars")]
        public async Task<ActionResult<List<CarDto>>> GetMyCars()
        {
            // Obtenemos el ID del usuario directamente del Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized(new { message = "Usuario no autenticado." });

            int userId = int.Parse(userIdClaim);
            
            // Usamos el método correcto de tu servicio
            var cars = await _carService.GetCarsByUserIdAsync(userId);
            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> GetById(int id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null) return NotFound(new { message = "Coche no encontrado." });
            
            return Ok(car);
        }

        [HttpPost]
        public async Task<ActionResult<CarDto>> Create([FromBody] CarDtin carDtin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            try
            {
                var newCar = await _carService.AddCarAsync(userId, carDtin);
                return CreatedAtAction(nameof(GetById), new { id = newCar.Id }, newCar);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CarDtin carDtin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var success = await _carService.UpdateCarAsync(id, userId, carDtin);
            if (!success) return NotFound(new { message = "Coche no encontrado o no te pertenece." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var success = await _carService.DeleteCarAsync(id, userId);
            if (!success) return NotFound(new { message = "Coche no encontrado o no te pertenece." });

            return NoContent();
        }
    }
}
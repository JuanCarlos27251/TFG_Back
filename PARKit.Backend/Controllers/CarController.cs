using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CarDtin;
using PARKit.Backend.Repositories;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarRepository _carRepository;

        public CarController(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        // 1. Obtener los coches del usuario autenticado (Tu método original corregido)
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetMyCars()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido o usuario no autenticado." });
            }

            var cars = await _carRepository.GetCarsByUserIdAsync(userId);
            return Ok(cars);
        }

        // 2. Obtener un coche específico por su ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> GetCarById(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound(new { message = $"No se encontró ningún vehículo con ID {id}." });
            }
            return Ok(car);
        }

        // 3. Registrar un nuevo coche asignado a un usuario
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<CarDto>> AddCar(int userId, [FromBody] CarDtin carDtin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newCar = await _carRepository.AddAsync(userId, carDtin);
                return CreatedAtAction(nameof(GetCarById), new { id = newCar.Id }, newCar);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al registrar el vehículo.", error = ex.Message });
            }
        }

        // 4. Actualizar un coche (Requiere ID del coche y del usuario por seguridad)
        [HttpPut("{id}/user/{userId}")]
        public async Task<IActionResult> UpdateCar(int id, int userId, [FromBody] CarDtin carDtin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _carRepository.UpdateAsync(id, userId, carDtin);
            if (!updated)
            {
                return NotFound(new { message = "No se pudo actualizar. Verifica que el coche exista y pertenezca al usuario." });
            }

            return NoContent();
        }

        // 5. Eliminar un coche
        [HttpDelete("{id}/user/{userId}")]
        public async Task<IActionResult> DeleteCar(int id, int userId)
        {
            var deleted = await _carRepository.DeleteAsync(id, userId);
            if (!deleted)
            {
                return NotFound(new { message = "No se pudo eliminar. Verifica que el coche exista y pertenezca al usuario." });
            }

            return NoContent();
        }
        
    }
}
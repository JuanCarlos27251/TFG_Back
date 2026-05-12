using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Crea una nueva reserva calculando el precio automáticamente según el coche y el parking.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] ReservationDtin dtin)
        {
            try
            {
                var result = await _reservationService.CreateReservationAsync(dtin);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // Error de lógica de negocio (ej: plaza ocupada)
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // No se encontró el coche, la plaza o el parking
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los detalles de una reserva específica.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetById(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        /// <summary>
        /// Obtiene todas las reservas de un usuario.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByUser(int userId)
        {
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }

        /// <summary>
        /// Cancela una reserva cambiando su estado.
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var success = await _reservationService.CancelReservationAsync(id);
            if (!success) return NotFound(new { message = "No se pudo encontrar la reserva para cancelar." });
            
            return NoContent();
        }
    }
}
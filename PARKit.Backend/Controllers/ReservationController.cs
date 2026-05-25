using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ReservationDtin;
using PARKit.Backend.Services.AuthServices;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IAuthServices _authService;
 
        public ReservationController(IReservationService reservationService, IAuthServices authService)
        {
            _reservationService = reservationService;
            _authService = authService;
        }
 
        /// <summary>
        /// Crea una nueva reserva. El UserId se fuerza desde el token JWT,
        /// ignorando lo que venga en el body para evitar suplantación.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] ReservationDtin dtin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
 
            // Sobrescribimos el UserId con el del token, el cliente no puede falsificarlo
            dtin.UserId = int.Parse(userIdClaim);
 
            try
            {
                var result = await _reservationService.CreateReservationAsync(dtin);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado.", detail = ex.Message });
            }
        }
 
        /// <summary>
        /// Obtiene los detalles de una reserva. Solo el dueño de la reserva o un Admin pueden verla.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetById(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound(new { message = "Reserva no encontrada." });
 
            // Verificamos que el usuario que pide sea el dueño o un Admin
            if (!_authService.HasAccessToResource(reservation.UserId, User))
                return Forbid();
 
            return Ok(reservation);
        }
 
        /// <summary>
        /// Obtiene todas las reservas de un usuario. Solo el propio usuario o un Admin pueden listarlas.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByUser(int userId)
        {
            if (!_authService.HasAccessToResource(userId, User))
                return Forbid();
 
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }
 
        /// <summary>
        /// Obtiene todas las reservas del usuario autenticado directamente desde el token.
        /// Alternativa más cómoda a GET /user/{userId} para el frontend.
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMy()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
 
            int userId = int.Parse(userIdClaim);
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }
 
        /// <summary>
        /// Cancela una reserva. Solo el dueño de la reserva o un Admin pueden cancelarla.
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            // Primero recuperamos la reserva para saber a quién pertenece
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound(new { message = "Reserva no encontrada." });
 
            if (!_authService.HasAccessToResource(reservation.UserId, User))
                return Forbid();
 
            var success = await _reservationService.CancelReservationAsync(id);
            if (!success) return BadRequest(new { message = "No se pudo cancelar la reserva." });
 
            return NoContent();
        }
    }
}
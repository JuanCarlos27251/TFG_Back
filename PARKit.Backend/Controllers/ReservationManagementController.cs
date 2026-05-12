using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class ReservationManagementController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationManagementController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Obtiene todas las reservas de los parkings asociados a una compañía.
        /// Uso principal: Dashboard del Manager.
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByCompany(int companyId)
        {
            var reservations = await _reservationService.GetByCompanyAsync(companyId);
            return Ok(reservations);
        }

        /// <summary>
        /// Obtiene el listado global de todas las reservas del sistema.
        /// Uso principal: Panel de administración global (Admin).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }

        /// <summary>
        /// Obtiene el detalle de una reserva específica por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetById(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound(new { message = "Reserva no encontrada." });
            
            return Ok(reservation);
        }

        /// <summary>
        /// Permite a un gestor cancelar una reserva manualmente (por ejemplo, por mantenimiento).
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var result = await _reservationService.CancelReservationAsync(id);
            if (!result) return BadRequest(new { message = "No se pudo cancelar la reserva." });

            return Ok(new { message = "Reserva cancelada correctamente." });
        }
    }
}
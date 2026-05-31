using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs.PaymentDtin;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetByReservation(int reservationId)
        {
            var payment = await _paymentService.GetByReservationAsync(reservationId);
            if (payment == null) return NotFound("No se encontró pago para esta reserva.");
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentDtin dtin)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(dtin);
                return Ok(result);
            }
            catch (Stripe.StripeException ex)
            {
                return BadRequest(new { message = "Error con Stripe: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error inesperado: " + ex.Message });
            }
        }

        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id, [FromQuery] string transactionId)
        {
            var success = await _paymentService.ConfirmPaymentAsync(id, transactionId);
            if (!success) return BadRequest(new { message = "No se pudo confirmar el pago." });
            return Ok(new { message = "Pago confirmado correctamente." });
        }
    }
}
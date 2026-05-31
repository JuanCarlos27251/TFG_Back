using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs.PaymentMethodDtin;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMethodController(IPaymentMethodService methodService) : ControllerBase
    {
       private readonly IPaymentMethodService _methodService = methodService;

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var methods = await _methodService.GetUserMethodsAsync(userId);
            return Ok(methods);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PaymentMethodDtin dtin)
        {
            var result = await _methodService.AddMethodAsync(dtin);
            return CreatedAtAction(nameof(GetByUser), new { userId = dtin.UserId }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _methodService.DeleteMethodAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
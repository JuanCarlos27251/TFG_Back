using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager, Admin")]
    public class ParkingManegementController : ControllerBase
    {
        private readonly IParkingService _parkingService;

        public ParkingManegementController(IParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        [HttpPost]
        public async Task<ActionResult<ParkingDto>> Create([FromBody] ParkingDtin dtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _parkingService.CreateParkingAsync(dtin);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ParkingDtin dtin)
        {
            var success = await _parkingService.UpdateParkingAsync(id, dtin);
            if (!success) return NotFound(new { message = "No se pudo actualizar el parking" });
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _parkingService.DeleteParkingAsync(id);
            if (!success) return NotFound(new { message = "No se pudo eliminar el parking" });
            
            return NoContent();
        }
    }

}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.ParkingDtin;
using PARKit.Backend.DTOs.TarifDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
   [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class ParkingManagementController : ControllerBase 
    {
        private readonly IParkingService _parkingService;
        private readonly IParkingSpotService _spotService;
        private readonly ITarifService _tarifService;

        public ParkingManagementController(
            IParkingService parkingService, 
            IParkingSpotService spotService, 
            ITarifService tarifService)
        {
            _parkingService = parkingService;
            _spotService = spotService;
            _tarifService = tarifService;
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

        // --- GESTIÓN DE PLAZAS (PARKING SPOTS - Añadido Fase 1) ---

        [HttpGet("parking/{parkingId}/spots")]
        public async Task<ActionResult<IEnumerable<ParkingSpotDto>>> GetSpotsByParking(int parkingId)
        {
            var spots = await _spotService.GetSpotsByParkingIdAsync(parkingId);
            return Ok(spots);
        }

        [HttpPost("spots")]
        public async Task<ActionResult<ParkingSpotDto>> AddSpot([FromBody] ParkingSpotDtin dtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var newSpot = await _spotService.AddSpotAsync(dtin);
                return Ok(newSpot);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("spots/{id}/status")]
        public async Task<IActionResult> UpdateSpotStatus(int id, [FromQuery] SpotStatus status)
        {
            var updated = await _spotService.UpdateSpotStatusAsync(id, status);
            if (!updated) return NotFound(new { message = "No se pudo actualizar el estado de la plaza." });
            return NoContent();
        }

        [HttpDelete("spots/{id}")]
        public async Task<IActionResult> DeleteSpot(int id)
        {
            var deleted = await _spotService.DeleteSpotAsync(id);
            if (!deleted) return NotFound(new { message = "No se pudo encontrar o eliminar la plaza." });
            return NoContent();
        }

        // --- GESTIÓN DE TARIFAS (TARIFS - Añadido Fase 1) ---

        [HttpGet("parking/{parkingId}/tarifs")]
        public async Task<ActionResult<IEnumerable<TarifDto>>> GetTarifsByParking(int parkingId)
        {
            var tarifs = await _tarifService.GetTarifsByParkingIdAsync(parkingId);
            return Ok(tarifs);
        }

        [HttpPost("tarifs")]
        public async Task<ActionResult<TarifDto>> AddTarif([FromBody] TarifDtin dtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var newTarif = await _tarifService.AddTarifAsync(dtin);
                return Ok(newTarif);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("tarifs/{id}")]
        public async Task<IActionResult> DeleteTarif(int id)
        {
            var deleted = await _tarifService.DeleteTarifAsync(id);
            if (!deleted) return NotFound(new { message = "No se pudo eliminar la tarifa." });
            return NoContent();
        }
    }

}
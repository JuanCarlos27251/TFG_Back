using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CompanyDtin;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        // Permitimos registro anónimo para que las nuevas empresas puedan darse de alta
        [HttpPost("register")]
        [AllowAnonymous] 
        public async Task<ActionResult<CompanyDto>> Register([FromBody] CompanyDtin dtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _companyService.CreateCompanyAsync(dtin);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] 
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        [Authorize] // Requiere estar logueado
        public async Task<ActionResult<CompanyDto>> GetById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return NotFound(new { message = "Empresa no encontrada." });
            return Ok(company);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyDtin dtin)
        {
            try
            {
                var success = await _companyService.UpdateCompanyAsync(id, dtin);
                if (!success) return NotFound(new { message = "Empresa no encontrada." });
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Mejor que solo un Admin borre empresas
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _companyService.DeleteCompanyAsync(id);
            if (!success) return NotFound(new { message = "Empresa no encontrada." });
            return NoContent();
        }
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Services.AuthServices;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Controllers
{
  [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthServices _authService;
 
        public UserController(IUserService userService, IAuthServices authService)
        {
            _userService = userService;
            _authService = authService;
        }
 
   
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
 
    
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
 
            int userId = int.Parse(userIdClaim);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Usuario no encontrado." });
 
            return Ok(user);
        }
 
 
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            if (!_authService.HasAccessToResource(id, User))
                return Forbid();
 
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "Usuario no encontrado." });
 
            return Ok(user);
        }
 

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserDtin dtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
 
            if (!_authService.HasAccessToResource(id, User))
                return Forbid();
 
            try
            {
                var success = await _userService.UpdateUserAsync(id, dtin);
                if (!success) return NotFound(new { message = "Usuario no encontrado." });
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
 

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_authService.HasAccessToResource(id, User))
                return Forbid();
 
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound(new { message = "Usuario no encontrado." });
 
            return NoContent();
        }
    }
}
 
using Microsoft.AspNetCore.Mvc;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Services;
using PARKit.Backend.Services.AuthServices;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authService;

        public AuthController(IAuthServices authService)
        {
            _authService = authService;
        }

         [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDtin loginDtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var token =  await _authService.Login(loginDtin);
                return Ok(new {Token = token});

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest
                ("Error generating token: " + ex.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDtin userDtin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }
                var token = await _authService.Register(userDtin);
                return Ok(new {Token = token});

            }
            catch (Exception ex)
            {
                return BadRequest
                ("Error generating token: " + ex.Message);
            }
        }
    }
}
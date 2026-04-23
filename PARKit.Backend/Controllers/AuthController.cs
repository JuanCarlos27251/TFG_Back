using Microsoft.AspNetCore.Mvc;

namespace PArRKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authServices;

        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

         [HttpPost("Login")]
        public IActionResult Login(LoginDtin loginDtin)
        {
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }
                var token = _authServices.Login(loginDtin);
                return Ok(token);

            }
            catch (KeyNotFoundException ex)
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
        public IActionResult Register(UserDtin userDtin)
        {
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }
                var token = _authServices.Register(userDtin);
                return Ok(token);

            }
            catch (Exception ex)
            {
                return BadRequest
                ("Error generating token: " + ex.Message);
            }
        }
    }
}
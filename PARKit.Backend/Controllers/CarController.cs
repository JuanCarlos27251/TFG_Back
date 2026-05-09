using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PARKit.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarServices _carServices;
        public CarController(ICarServices carServices)
        {
            _carServices = carServices;
        }

        [HttpGet ("Car")]
         public async Task <IActionResult<List<CarDto>>> GetAll()
        {
            var car = await _carServices.GetAllAsync();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(userId != null)
                {
                    car = car.Where (c => c.IdUsuario == int.Parse(userId)).ToList();
                }
            }
            return Ok(car);
        }
        
    }
}
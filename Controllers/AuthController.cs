using DigitalniCjenik.DTO;
using DigitalniCjenik.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequestDTO request)
        {
            var result = _authService.Login(request);
            if (result == null)
                return Unauthorized();

            return Ok(result);
        }
    }
}

using DigitalniCjenik.DTO;
using DigitalniCjenik.Services;
using Microsoft.AspNetCore.Authorization;
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
                return Unauthorized("Neispravan email ili lozinka");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDTO request)
        {
            // Provjera da li su svi podaci uneseni
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Lozinka) || string.IsNullOrEmpty(request.ImePrezime))
                return BadRequest("Email, lozinka i ime su obavezni.");

            var success = await _authService.Register(request);

            if (!success)
                return BadRequest("Korisnik s tim emailom već postoji.");

            return Ok("Registracija uspješna.");
        }

    }
}

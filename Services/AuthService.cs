using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using DigitalniCjenik.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalniCjenik.Services
{
    public class AuthService
    {
        private readonly DigitalniCjenikContext _context;
        private readonly JwtSettings _jwt;

        public AuthService(DigitalniCjenikContext context, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _jwt = jwtOptions.Value;
        }

        public LoginResponseDTO? Login(LoginRequestDTO request)
        {
            var korisnik = _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefault(k => k.Email == request.Email);

            if (korisnik == null)
                return null;

            if (request.Lozinka == null || korisnik.LozinkaHash == null || korisnik.LozinkaSalt == null)
                return null;

            var isValidPassword = PasswordHasher.VerifyPassword(
                request.Lozinka,
                korisnik.LozinkaHash,
                korisnik.LozinkaSalt
            );

            if (!isValidPassword)
                return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, korisnik.ID.ToString()),
                new Claim(ClaimTypes.Email, korisnik.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, korisnik.Uloga?.Naziv ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.Key ?? string.Empty)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                _jwt.Issuer,
                _jwt.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(_jwt.ExpirationMinutes),
                signingCredentials: creds
            );

            return new LoginResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Uloga = korisnik.Uloga?.Naziv ?? string.Empty,
                ImePrezime = korisnik.ImePrezime
            };

        }

        public async Task<bool> Register(RegisterRequestDTO request)
        {
            // Provjera da li već postoji korisnik s istim emailom
            if (_context.Korisnici.Any(k => k.Email == request.Email))
                return false;

            // Provjera da li je lozinka null
            if (string.IsNullOrEmpty(request.Lozinka))
                return false;

            // Generiranje hash i salt koristeći PasswordHasher
            PasswordHasher.CreatePasswordHash(
                request.Lozinka,
                out byte[] passwordHash,
                out byte[] passwordSalt
            );

            // Kreiranje novog korisnika
            var korisnik = new Korisnik
            {
                ImePrezime = request.ImePrezime,
                Email = request.Email,
                LozinkaHash = passwordHash,
                LozinkaSalt = passwordSalt,
                UlogaID = request.UlogaID,
                JezikSučelja = "HR",
                Aktivnost = true
            };

            _context.Korisnici.Add(korisnik);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}

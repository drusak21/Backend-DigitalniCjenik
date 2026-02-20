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
        private readonly LdapSettings _ldapSettings;
        private readonly ILdapService _ldapService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            DigitalniCjenikContext context,
            IOptions<JwtSettings> jwtOptions,
            IOptions<LdapSettings> ldapOptions,
            ILdapService ldapService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwt = jwtOptions.Value;
            _ldapSettings = ldapOptions.Value;
            _ldapService = ldapService;
            _logger = logger;
        }

        public LoginResponseDTO? Login(LoginRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Lozinka))
                return null;

            // Odredi način prijave
            bool useLdap = _ldapSettings.EnableLdap && _ldapService.IsLdapUser(request.Email);

            _logger.LogInformation("Login attempt for {Email} using {Method}",
                request.Email, useLdap ? "LDAP" : "Local");

            if (useLdap)
            {
                return HandleLdapLogin(request.Email, request.Lozinka);
            }
            else
            {
                return HandleLocalLogin(request.Email, request.Lozinka);
            }
        }

        private LoginResponseDTO? HandleLocalLogin(string email, string lozinka)
        {
            var korisnik = _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefault(k => k.Email == email);

            if (korisnik == null)
            {
                _logger.LogWarning("Local login failed - user not found: {Email}", email);
                return null;
            }

            if (korisnik.LozinkaHash == null || korisnik.LozinkaSalt == null)
            {
                _logger.LogWarning("Local login failed - no password hash: {Email}", email);
                return null;
            }

            var isValidPassword = PasswordHasher.VerifyPassword(
                lozinka,
                korisnik.LozinkaHash,
                korisnik.LozinkaSalt
            );

            if (!isValidPassword)
            {
                _logger.LogWarning("Local login failed - invalid password: {Email}", email);
                return null;
            }

            _logger.LogInformation("Local login successful: {Email}", email);
            return GenerateTokenResponse(korisnik);
        }

        private LoginResponseDTO? HandleLdapLogin(string email, string lozinka)
        {
            try
            {
                // 1. LDAP autentifikacija (mock)
                var ldapUser = _ldapService.AuthenticateAsync(email, lozinka).Result;

                if (ldapUser == null)
                {
                    _logger.LogWarning("LDAP login failed for {Email}", email);
                    return null;
                }

                // 2. Pronađi ili kreiraj korisnika u lokalnoj bazi
                var korisnik = FindOrCreateLdapUser(ldapUser);

                // 3. Generiraj JWT token
                _logger.LogInformation("LDAP login successful: {Email}", email);
                return GenerateTokenResponse(korisnik);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during LDAP login for {Email}", email);
                return null;
            }
        }

        private Korisnik FindOrCreateLdapUser(LdapUserInfo ldapUser)
        {
            // Pokušaj naći korisnika po emailu
            var korisnik = _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefault(k => k.Email == ldapUser.Email);

            if (korisnik == null)
            {
                _logger.LogInformation("Creating new user from LDAP: {Email}", ldapUser.Email);

                // Dohvati UlogaID iz mock servisa (ako postoji)
                int? ulogaId = null;
                if (_ldapService is MockLdapService mockLdap && !string.IsNullOrEmpty(ldapUser.Username))
                {
                    ulogaId = mockLdap.GetUlogaIdForUser(ldapUser.Username);
                }

                // Ako nije nađen, koristi default (3)
                int defaultUlogaID = ulogaId ?? 3;

                korisnik = new Korisnik
                {
                    ImePrezime = ldapUser.ImePrezime ?? ldapUser.Username ?? "LDAP User",
                    Email = ldapUser.Email ?? $"{ldapUser.Username}@{_ldapSettings.Domain}",
                    LozinkaHash = null,
                    LozinkaSalt = null,
                    UlogaID = defaultUlogaID, 
                    JezikSučelja = "HR",
                    Aktivnost = true
                };

                _context.Korisnici.Add(korisnik);
                _context.SaveChanges();
                _logger.LogInformation("New LDAP user created with ID: {UserId}, UlogaID: {UlogaID}",
                    korisnik.ID, korisnik.UlogaID);
            }

            return korisnik;
        }

        private LoginResponseDTO GenerateTokenResponse(Korisnik korisnik)
        {
            string ulogaNaziv = korisnik.Uloga?.Naziv ?? "";

            if (string.IsNullOrEmpty(ulogaNaziv) && korisnik.UlogaID > 0)
            {

                var uloga = _context.Uloge.Find(korisnik.UlogaID);
                ulogaNaziv = uloga?.Naziv ?? "";
            }

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
            // Spriječi registraciju ako je email u LDAP domeni
            if (_ldapSettings.EnableLdap && _ldapService.IsLdapUser(request.Email ?? ""))
            {
                _logger.LogWarning("Attempted to register LDAP user {Email}", request.Email);
                return false;
            }

            // Provjera da li već postoji korisnik s istim emailom
            if (_context.Korisnici.Any(k => k.Email == request.Email))
                return false;

            // Provjera da li je lozinka null
            if (string.IsNullOrEmpty(request.Lozinka))
                return false;

            // Generiranje hash i salt
            PasswordHasher.CreatePasswordHash(
                request.Lozinka,
                out byte[] passwordHash,
                out byte[] passwordSalt
            );

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

            _logger.LogInformation("New local user registered: {Email}", request.Email);
            return true;
        }
    }
}
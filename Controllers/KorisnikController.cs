using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using DigitalniCjenik.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/korisnici")]
    [Authorize(Roles = "Administrator")] // Samo admin može upravljati korisnicima
    public class KorisnikController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public KorisnikController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        // GET: api/korisnici
        [HttpGet]
        public async Task<IActionResult> GetKorisnici()
        {
             var korisnici = await _context.Korisnici
            .Include(k => k.Uloga)
            .Select(k => new KorisnikDTO
            {
                ID = k.ID,
                ImePrezime = k.ImePrezime!,
                Email = k.Email!,
                UlogaNaziv = k.Uloga != null && k.Uloga.Naziv != null ? k.Uloga.Naziv : string.Empty,
                OpisUloge = k.Uloga != null && k.Uloga.OpisPrava != null ? k.Uloga.OpisPrava : string.Empty,
                JezikSučelja = k.JezikSučelja,
                Aktivnost = k.Aktivnost
            })
            .ToListAsync();

         return Ok(korisnici);
        }

        // POST: api/korisnici
        [HttpPost]
        public async Task<IActionResult> CreateKorisnik(KorisnikCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Lozinka) || string.IsNullOrEmpty(dto.ImePrezime))
                return BadRequest("Ime, email i lozinka su obavezni.");

            if (await _context.Korisnici.AnyAsync(k => k.Email == dto.Email))
                return BadRequest("Korisnik s tim emailom već postoji.");

            PasswordHasher.CreatePasswordHash(dto.Lozinka!, out byte[] hash, out byte[] salt);

            var korisnik = new Korisnik
            {
                ImePrezime = dto.ImePrezime,
                Email = dto.Email,
                LozinkaHash = hash,
                LozinkaSalt = salt,
                UlogaID = dto.UlogaID,
                Aktivnost = true
            };

            _context.Korisnici.Add(korisnik);
            await _context.SaveChangesAsync();

            return Ok("Korisnik uspješno kreiran.");
        }

        // PUT: api/korisnici/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKorisnik(int id, KorisnikUpdateDTO dto)
        {
            var korisnik = await _context.Korisnici.FindAsync(id);
            if (korisnik == null)
                return NotFound("Korisnik ne postoji.");

            if (!string.IsNullOrEmpty(dto.ImePrezime))
                korisnik.ImePrezime = dto.ImePrezime;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (await _context.Korisnici.AnyAsync(k => k.Email == dto.Email && k.ID != id))
                    return BadRequest("Email je već zauzet.");
                korisnik.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Lozinka))
                PasswordHasher.CreatePasswordHash(dto.Lozinka, out byte[] hash, out byte[] salt);

            if (dto.Aktivnost.HasValue)
                korisnik.Aktivnost = dto.Aktivnost.Value;

            korisnik.UlogaID = dto.UlogaID;

            _context.Korisnici.Update(korisnik);
            await _context.SaveChangesAsync();

            return Ok("Korisnik uspješno ažuriran.");
        }

        // PATCH: api/korisnici/{id}/deactivate
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateKorisnik(int id)
        {
            var korisnik = await _context.Korisnici.FindAsync(id);
            if (korisnik == null)
                return NotFound("Korisnik ne postoji.");

            korisnik.Aktivnost = false;
            _context.Korisnici.Update(korisnik);
            await _context.SaveChangesAsync();

            return Ok("Korisnik deaktiviran.");
        }
    }
}

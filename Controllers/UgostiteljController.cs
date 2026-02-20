using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/ugostitelji")]
    [Authorize(Roles = "Administrator")]
    public class UgostiteljController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public UgostiteljController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUgostitelji()
        {
            var ugostitelji = await _context.Ugostitelji
                .Select(u => new UgostiteljDTO
                {
                    ID = u.ID,
                    Naziv = u.Naziv,
                    OIB = u.OIB,
                    KorisnikID = u.KorisnikID
                })
                .ToListAsync();

            return Ok(ugostitelji);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUgostitelj(int id)
        {
            var u = await _context.Ugostitelji.FindAsync(id);
            if (u == null)
                return NotFound("Ugostitelj ne postoji.");

            return Ok(new UgostiteljDTO
            {
                ID = u.ID,
                Naziv = u.Naziv,
                OIB = u.OIB,
                KorisnikID = u.KorisnikID,
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUgostitelj(UgostiteljCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Naziv))
                return BadRequest("Naziv je obavezan.");

            var korisnik = await _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefaultAsync(k => k.ID == dto.KorisnikID);

            if (korisnik == null)
                return BadRequest("Korisnik ne postoji.");

            if (korisnik.Uloga == null || korisnik.Uloga.Naziv != "Ugostitelj")
                return BadRequest("Korisnik nije u ulozi Ugostitelj.");

            var ugostitelj = new Ugostitelj
            {
                Naziv = dto.Naziv,
                OIB = dto.OIB,
                KontaktEmail = dto.KontaktEmail,
                KontaktTelefon = dto.KontaktTelefon,
                KorisnikID = dto.KorisnikID,
            };

            _context.Ugostitelji.Add(ugostitelj);
            await _context.SaveChangesAsync();

            return Ok("Ugostitelj uspješno kreiran.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUgostitelj(int id, UgostiteljUpdateDTO dto)
        {
            var u = await _context.Ugostitelji.FindAsync(id);
            if (u == null)
                return NotFound("Ugostitelj ne postoji.");

            if (!string.IsNullOrEmpty(dto.Naziv))
                u.Naziv = dto.Naziv;

            if (!string.IsNullOrEmpty(dto.OIB))
                u.OIB = dto.OIB;

            _context.Ugostitelji.Update(u);
            await _context.SaveChangesAsync();

            return Ok("Ugostitelj ažuriran.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUgostitelj(int id)
        {
            try
            {
                var ugostitelj = await _context.Ugostitelji
                    .Include(u => u.Objekti) 
                    .FirstOrDefaultAsync(u => u.ID == id);

                if (ugostitelj == null)
                    return NotFound("Ugostitelj nije pronađen.");

                if (ugostitelj.Objekti != null && ugostitelj.Objekti.Any())
                {
                    var povezaniObjekti = ugostitelj.Objekti
                        .Select(o => new { o.ID, o.Naziv })
                        .ToList();

                    return BadRequest(new
                    {
                        poruka = "Ugostitelj se ne može obrisati jer ima povezane objekte.",
                        objekti = povezaniObjekti
                    });
                }

                _context.Ugostitelji.Remove(ugostitelj);
                await _context.SaveChangesAsync();

                return Ok("Ugostitelj uspješno obrisan.");
            }
            catch (Exception )
            {
                return StatusCode(500, "Došlo je do greške pri brisanju ugostitelja.");
            }
        }


    }
}

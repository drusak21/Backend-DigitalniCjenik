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

            if (!await _context.Korisnici.AnyAsync(k => k.ID == dto.KorisnikID))
                return BadRequest("Korisnik ne postoji.");

            var ugostitelj = new Ugostitelj
            {
                Naziv = dto.Naziv,
                OIB = dto.OIB,
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


    }
}

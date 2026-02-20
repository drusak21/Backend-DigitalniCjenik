using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/kategorije")]
    [Authorize]
    public class KategorijaController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public KategorijaController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetKategorije()
        {
            var kategorije = await _context.Kategorije
                .OrderBy(k => k.RedoslijedPrikaza)
                .Select(k => new KategorijaReadDTO
                {
                    ID = k.ID,
                    Naziv = k.Naziv!,
                    RedoslijedPrikaza = k.RedoslijedPrikaza,
                    Aktivan = k.Aktivan
                })
                .ToListAsync();

            return Ok(kategorije);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKategorija(int id)
        {
            var kategorija = await _context.Kategorije
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kategorija == null)
                return NotFound("Kategorija ne postoji.");

            var dto = new KategorijaReadDTO
            {
                ID = kategorija.ID,
                Naziv = kategorija.Naziv!,
                RedoslijedPrikaza = kategorija.RedoslijedPrikaza,
                Aktivan = kategorija.Aktivan
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateKategorija(KategorijaCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Naziv))
                return BadRequest("Naziv kategorije je obavezan.");

            var postoji = await _context.Kategorije
                .AnyAsync(k => k.Naziv != null && EF.Functions.ILike(k.Naziv, dto.Naziv!));

            if (postoji)
                return BadRequest("Kategorija s tim nazivom već postoji.");

            var kategorija = new Kategorija
            {
                Naziv = dto.Naziv,
                RedoslijedPrikaza = dto.RedoslijedPrikaza,
                Aktivan = true
            };

            _context.Kategorije.Add(kategorija);
            await _context.SaveChangesAsync();

            var readDto = new KategorijaReadDTO
            {
                ID = kategorija.ID,
                Naziv = kategorija.Naziv!,
                RedoslijedPrikaza = kategorija.RedoslijedPrikaza,
                Aktivan = kategorija.Aktivan
            };

            return CreatedAtAction(nameof(GetKategorija), new { id = kategorija.ID }, readDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateKategorija(int id, KategorijaUpdateDTO dto)
        {
            if (id != dto.ID)
                return BadRequest("ID u URL-u ne odgovara ID-u u tijelu zahtjeva.");

            var kategorija = await _context.Kategorije.FindAsync(id);
            if (kategorija == null)
                return NotFound("Kategorija ne postoji.");

           
            if (!string.IsNullOrWhiteSpace(dto.Naziv) &&
                !string.Equals(dto.Naziv, kategorija.Naziv, StringComparison.OrdinalIgnoreCase))
            {
                var postoji = await _context.Kategorije
                    .AnyAsync(k => k.Naziv != null &&
                                  EF.Functions.ILike(k.Naziv, dto.Naziv) &&
                                  k.ID != id);

                if (postoji)
                    return BadRequest("Kategorija s tim nazivom već postoji.");
            }

            // Ažuriranje
            if (!string.IsNullOrWhiteSpace(dto.Naziv))
                kategorija.Naziv = dto.Naziv;

            if (dto.RedoslijedPrikaza.HasValue)
                kategorija.RedoslijedPrikaza = dto.RedoslijedPrikaza.Value;

            if (dto.Aktivan.HasValue)
                kategorija.Aktivan = dto.Aktivan.Value;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Kategorija uspješno ažurirana." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteKategorija(int id)
        {
            var kategorija = await _context.Kategorije
                .Include(k => k.Artikli)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kategorija == null)
                return NotFound("Kategorija ne postoji.");

            if (kategorija.Artikli != null && kategorija.Artikli.Any())
                return BadRequest("Ne može se obrisati kategorija koja sadrži artikle.");

            _context.Kategorije.Remove(kategorija);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kategorija obrisana." });
        }
    }
}

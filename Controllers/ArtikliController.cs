using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/artikli")]
    [Authorize]
    public class ArtikliController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public ArtikliController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        // GET: api/artikli
        [HttpGet]
        public async Task<IActionResult> GetArtikli()
        {
            var artikli = await _context.Artikli
                .Include(a => a.Kategorija)
                .Select(a => new ArtiklReadDTO
                {
                    ID = a.ID,
                    Naziv = a.Naziv,
                    Opis = a.Opis,
                    Cijena = a.Cijena,
                    SastavAlergeni = a.SastavAlergeni,
                    Slika = a.Slika,
                    Brand = a.Brand,
                    Zakljucan = a.Zakljucan,
                    KategorijaID = a.KategorijaID,
                    KategorijaNaziv = a.Kategorija != null ? a.Kategorija.Naziv : null
                })
                .ToListAsync();

            return Ok(artikli);
        }

        // POST: api/artikli
        [HttpPost]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> CreateArtikl(ArtiklCreateDTO dto)
        {
            var artikl = new Artikl
            {
                Naziv = dto.Naziv,
                Opis = dto.Opis,
                Cijena = dto.Cijena,
                SastavAlergeni = dto.SastavAlergeni,
                Slika = dto.Slika,
                Brand = dto.Brand,
                KategorijaID = dto.KategorijaID,
                Zakljucan = dto.Zakljucan
            };

            _context.Artikli.Add(artikl);
            await _context.SaveChangesAsync();

            return Ok("Artikl uspješno kreiran.");
        }

        // PUT: api/artikli/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> UpdateArtikl(int id, ArtiklUpdateDTO dto)
        {
            var artikl = await _context.Artikli.FindAsync(id);
            if (artikl == null)
                return NotFound("Artikl ne postoji.");

            if (artikl.Zakljucan)
                return BadRequest("Ovaj artikl je zaključan i ne može se uređivati.");

            if (dto.Naziv != null) artikl.Naziv = dto.Naziv;
            if (dto.Opis != null) artikl.Opis = dto.Opis;
            if (dto.Cijena.HasValue) artikl.Cijena = dto.Cijena.Value;
            if (dto.SastavAlergeni != null) artikl.SastavAlergeni = dto.SastavAlergeni;
            if (dto.Slika != null) artikl.Slika = dto.Slika;
            if (dto.Brand != null) artikl.Brand = dto.Brand;
            if (dto.KategorijaID.HasValue) artikl.KategorijaID = dto.KategorijaID;

            await _context.SaveChangesAsync();
            return Ok("Artikl uspješno ažuriran.");
        }

        // DELETE: api/artikli/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteArtikl(int id)
        {
            var artikl = await _context.Artikli.FindAsync(id);
            if (artikl == null)
                return NotFound("Artikl ne postoji.");

            if (artikl.Zakljucan)
                return BadRequest("Zaključani artikl se ne može obrisati.");

            _context.Artikli.Remove(artikl);
            await _context.SaveChangesAsync();

            return Ok("Artikl obrisan.");
        }

        // POST: api/artikli/import
        [HttpPost("import")]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("CSV datoteka nije poslana.");

            using var reader = new StreamReader(file.OpenReadStream());
            var lines = await reader.ReadToEndAsync();
            var rows = lines.Split('\n');

            foreach (var row in rows.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;

                var columns = row.Split(';');
                if (columns.Length < 3) continue;

                var naziv = columns[0].Trim();
                var cijena = decimal.Parse(columns[1], CultureInfo.InvariantCulture);
                var kategorijaNaziv = columns[2].Trim();

                var kategorija = await _context.Kategorije
                    .FirstOrDefaultAsync(k => k.Naziv == kategorijaNaziv);

                if (kategorija == null)
                {
                    kategorija = new Kategorija { Naziv = kategorijaNaziv };
                    _context.Kategorije.Add(kategorija);
                    await _context.SaveChangesAsync();
                }

                var artikl = new Artikl
                {
                    Naziv = naziv,
                    Cijena = cijena,
                    KategorijaID = kategorija.ID
                };

                _context.Artikli.Add(artikl);
            }

            await _context.SaveChangesAsync();
            return Ok("CSV import uspješno završen.");
        }
    }

}


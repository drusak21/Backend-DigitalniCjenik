using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/akcije")]
    [Authorize(Roles = "Administrator")]
    public class AkcijeController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public AkcijeController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        // GET: api/akcije
        [HttpGet]
        public async Task<IActionResult> GetAkcije([FromQuery] bool aktivneSamo = false)
        {
            var query = _context.Akcije
                .Include(a => a.Objekt)
                .AsQueryable();

            if (aktivneSamo)
            {
                var now = DateTime.UtcNow;
                query = query.Where(a => a.Aktivna &&
                    (!a.DatumPocetka.HasValue || a.DatumPocetka <= now) &&
                    (!a.DatumZavrsetka.HasValue || a.DatumZavrsetka >= now));
            }

            var akcije = await query
                .Select(a => new AkcijaDTO
                {
                    ID = a.ID,
                    Naziv = a.Naziv,
                    Opis = a.Opis,
                    Vrsta = a.Vrsta,
                    DatumPocetka = a.DatumPocetka,
                    DatumZavrsetka = a.DatumZavrsetka,
                    Slika = a.Slika,
                    Aktivna = a.Aktivna,
                    ObjektID = a.ObjektID,
                    ObjektNaziv = a.Objekt != null ? a.Objekt.Naziv : null
                })
                .ToListAsync();

            return Ok(akcije);
        }

        // GET: api/akcije/objekt/{objektId}
        [HttpGet("objekt/{objektId}")]
        [AllowAnonymous] // Za javni prikaz u cjeniku
        public async Task<IActionResult> GetAkcijeZaObjekt(int objektId)
        {
            var now = DateTime.UtcNow;
            var akcije = await _context.Akcije
                .Where(a => a.Aktivna &&
                    (a.ObjektID == null || a.ObjektID == objektId) &&
                    (!a.DatumPocetka.HasValue || a.DatumPocetka <= now) &&
                    (!a.DatumZavrsetka.HasValue || a.DatumZavrsetka >= now))
                .Select(a => new AkcijaDTO
                {
                    ID = a.ID,
                    Naziv = a.Naziv,
                    Opis = a.Opis,
                    Vrsta = a.Vrsta,
                    DatumPocetka = a.DatumPocetka,
                    DatumZavrsetka = a.DatumZavrsetka,
                    Slika = a.Slika,
                    Aktivna = a.Aktivna,
                    ObjektID = a.ObjektID,
                    ObjektNaziv = a.Objekt != null ? a.Objekt.Naziv : null
                })
                .ToListAsync();

            return Ok(akcije);
        }

        // GET: api/akcije/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAkcija(int id)
        {
            var akcija = await _context.Akcije
                .Include(a => a.Objekt)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (akcija == null)
                return NotFound("Akcija ne postoji.");

            var dto = new AkcijaDTO
            {
                ID = akcija.ID,
                Naziv = akcija.Naziv,
                Opis = akcija.Opis,
                Vrsta = akcija.Vrsta,
                DatumPocetka = akcija.DatumPocetka,
                DatumZavrsetka = akcija.DatumZavrsetka,
                Slika = akcija.Slika,
                Aktivna = akcija.Aktivna,
                ObjektID = akcija.ObjektID,
                ObjektNaziv = akcija.Objekt?.Naziv
            };

            return Ok(dto);
        }

        // POST: api/akcije
        [HttpPost]
        public async Task<IActionResult> CreateAkcija(AkcijaCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Naziv))
                return BadRequest("Naziv akcije je obavezan.");

            DateTime? pocetak = null;
            DateTime? zavrsetak = null;

            if (dto.DatumPocetka.HasValue)
            {

                pocetak = DateTime.SpecifyKind(dto.DatumPocetka.Value.Date, DateTimeKind.Utc);
            }

            if (dto.DatumZavrsetka.HasValue)
            {

                zavrsetak = DateTime.SpecifyKind(
                    dto.DatumZavrsetka.Value.Date.AddDays(1).AddSeconds(-1),
                    DateTimeKind.Utc
                );
            }

            var akcija = new Akcija
            {
                Naziv = dto.Naziv,
                Opis = dto.Opis,
                Vrsta = dto.Vrsta,
                DatumPocetka = pocetak,       
                DatumZavrsetka = zavrsetak,    
                Slika = dto.Slika,
                ObjektID = dto.ObjektID,
                Aktivna = true
            };

            _context.Akcije.Add(akcija);
            await _context.SaveChangesAsync();

            var kreiranaAkcija = await _context.Akcije
                .Include(a => a.Objekt)
                .FirstOrDefaultAsync(a => a.ID == akcija.ID);

            var resultDto = new AkcijaDTO
            {
                ID = kreiranaAkcija!.ID,
                Naziv = kreiranaAkcija.Naziv,
                Opis = kreiranaAkcija.Opis,
                Vrsta = kreiranaAkcija.Vrsta,
                DatumPocetka = kreiranaAkcija.DatumPocetka,
                DatumZavrsetka = kreiranaAkcija.DatumZavrsetka,
                Slika = kreiranaAkcija.Slika,
                Aktivna = kreiranaAkcija.Aktivna,
                ObjektID = kreiranaAkcija.ObjektID,
                ObjektNaziv = kreiranaAkcija.Objekt?.Naziv
            };

            return CreatedAtAction(nameof(GetAkcija), new { id = akcija.ID }, resultDto);
        }

        // PUT: api/akcije/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAkcija(int id, AkcijaCreateDTO dto)
        {
            var akcija = await _context.Akcije.FindAsync(id);
            if (akcija == null)
                return NotFound("Akcija ne postoji.");

            if (!string.IsNullOrWhiteSpace(dto.Naziv))
                akcija.Naziv = dto.Naziv;

            if (dto.Opis != null)
                akcija.Opis = dto.Opis;

            if (dto.Vrsta != null)
                akcija.Vrsta = dto.Vrsta;

            akcija.DatumPocetka = dto.DatumPocetka;
            akcija.DatumZavrsetka = dto.DatumZavrsetka;

            if (dto.Slika != null)
                akcija.Slika = dto.Slika;

            if (dto.ObjektID != akcija.ObjektID)
                akcija.ObjektID = dto.ObjektID;

            await _context.SaveChangesAsync();
            return Ok("Akcija uspješno ažurirana.");
        }

        // PATCH: api/akcije/{id}/aktiviraj
        [HttpPatch("{id}/aktiviraj")]
        public async Task<IActionResult> AktivirajAkciju(int id, [FromBody] bool aktivna)
        {
            var akcija = await _context.Akcije.FindAsync(id);
            if (akcija == null)
                return NotFound("Akcija ne postoji.");

            akcija.Aktivna = aktivna;
            await _context.SaveChangesAsync();

            return Ok($"Akcija {(aktivna ? "aktivirana" : "deaktivirana")}.");
        }

        // DELETE: api/akcije/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAkcija(int id)
        {
            var akcija = await _context.Akcije
                .Include(a => a.Banneri)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (akcija == null)
                return NotFound("Akcija ne postoji.");

            // Provjeri ima li povezanih bannera
            if (akcija.Banneri != null && akcija.Banneri.Any())
                return BadRequest("Ne može se obrisati akcija koja ima povezane bannere.");

            _context.Akcije.Remove(akcija);
            await _context.SaveChangesAsync();

            return Ok("Akcija obrisana.");
        }
    }
}


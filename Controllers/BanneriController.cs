using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/banneri")]
    public class BanneriController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public BanneriController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        // GET: api/banneri/objekt/{objektId}
        [HttpGet("objekt/{objektId}")]
        [AllowAnonymous] // Za javni prikaz u cjeniku
        public async Task<IActionResult> GetBanneriZaObjekt(int objektId, [FromQuery] string? tip = null)
        {
            var query = _context.Banneri
                .Include(b => b.Akcija)
                .Where(b => b.Aktivan && (b.ObjektID == null || b.ObjektID == objektId));

            if (!string.IsNullOrEmpty(tip))
                query = query.Where(b => b.Tip == tip);

            var banneri = await query
                .Select(b => new BannerDTO
                {
                    ID = b.ID,
                    Tip = b.Tip,
                    Sadrzaj = b.Sadrzaj,
                    Aktivan = b.Aktivan,
                    ObjektID = b.ObjektID,
                    AkcijaID = b.AkcijaID,
                    AkcijaNaziv = b.Akcija != null ? b.Akcija.Naziv : null
                })
                .ToListAsync();

            return Ok(banneri);
        }

        // GET: api/banneri
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetBanneri()
        {
            var banneri = await _context.Banneri
                .Include(b => b.Objekt)
                .Include(b => b.Akcija)
                .Select(b => new BannerDTO
                {
                    ID = b.ID,
                    Tip = b.Tip,
                    Sadrzaj = b.Sadrzaj,
                    Aktivan = b.Aktivan,
                    ObjektID = b.ObjektID,
                    ObjektNaziv = b.Objekt != null ? b.Objekt.Naziv : null,
                    AkcijaID = b.AkcijaID,
                    AkcijaNaziv = b.Akcija != null ? b.Akcija.Naziv : null
                })
                .ToListAsync();

            return Ok(banneri);
        }

        // GET: api/banneri/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var banner = await _context.Banneri
                .Include(b => b.Objekt)
                .Include(b => b.Akcija)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (banner == null)
                return NotFound("Banner ne postoji.");

            var dto = new BannerDTO
            {
                ID = banner.ID,
                Tip = banner.Tip,
                Sadrzaj = banner.Sadrzaj,
                Aktivan = banner.Aktivan,
                ObjektID = banner.ObjektID,
                ObjektNaziv = banner.Objekt?.Naziv,
                AkcijaID = banner.AkcijaID,
                AkcijaNaziv = banner.Akcija?.Naziv
            };

            return Ok(dto);
        }

        // POST: api/banneri
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateBanner(BannerCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tip))
                return BadRequest("Tip bannera je obavezan.");

            if (string.IsNullOrWhiteSpace(dto.Sadrzaj))
                return BadRequest("Sadržaj bannera je obavezan.");

            var banner = new Banner
            {
                Tip = dto.Tip,
                Sadrzaj = dto.Sadrzaj,
                ObjektID = dto.ObjektID,
                AkcijaID = dto.AkcijaID,
                Aktivan = true
            };

            _context.Banneri.Add(banner);
            await _context.SaveChangesAsync();

            var kreiraniBanner = await _context.Banneri
                .Include(b => b.Objekt)
                .Include(b => b.Akcija)
                .FirstOrDefaultAsync(b => b.ID == banner.ID);

            var resultDto = new BannerDTO
            {
                ID = kreiraniBanner!.ID,
                Tip = kreiraniBanner.Tip,
                Sadrzaj = kreiraniBanner.Sadrzaj,
                Aktivan = kreiraniBanner.Aktivan,
                ObjektID = kreiraniBanner.ObjektID,
                ObjektNaziv = kreiraniBanner.Objekt?.Naziv,
                AkcijaID = kreiraniBanner.AkcijaID,
                AkcijaNaziv = kreiraniBanner.Akcija?.Naziv
            };

            return CreatedAtAction(nameof(GetBanner), new { id = banner.ID }, resultDto);
        }
        // PUT: api/banneri/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateBanner(int id, BannerCreateDTO dto)
        {
            var banner = await _context.Banneri.FindAsync(id);
            if (banner == null)
                return NotFound("Banner ne postoji.");

            // Ako je vezan uz akciju, provjeri da akcija postoji
            if (dto.AkcijaID.HasValue && dto.AkcijaID != banner.AkcijaID)
            {
                var akcija = await _context.Akcije.FindAsync(dto.AkcijaID);
                if (akcija == null)
                    return BadRequest("Akcija ne postoji.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Tip))
                banner.Tip = dto.Tip;

            if (!string.IsNullOrWhiteSpace(dto.Sadrzaj))
                banner.Sadrzaj = dto.Sadrzaj;

            if (dto.ObjektID != banner.ObjektID)
                banner.ObjektID = dto.ObjektID;

            banner.AkcijaID = dto.AkcijaID;

            await _context.SaveChangesAsync();
            return Ok("Banner uspješno ažuriran.");
        }

        // PATCH: api/banneri/{id}/aktiviraj
        [HttpPatch("{id}/aktiviraj")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AktivirajBanner(int id, [FromBody] bool aktivan)
        {
            var banner = await _context.Banneri.FindAsync(id);
            if (banner == null)
                return NotFound("Banner ne postoji.");

            banner.Aktivan = aktivan;
            await _context.SaveChangesAsync();

            return Ok($"Banner {(aktivan ? "aktiviran" : "deaktiviran")}.");
        }

        // DELETE: api/banneri/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banneri.FindAsync(id);
            if (banner == null)
                return NotFound("Banner ne postoji.");

            _context.Banneri.Remove(banner);
            await _context.SaveChangesAsync();

            return Ok("Banner obrisan.");
        }
    }
}
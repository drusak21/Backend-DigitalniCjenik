using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/cjenici")]
    [Authorize]
    public class CjeniciController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public CjeniciController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        // GET: api/cjenici/objekt/{objektId}
        [HttpGet("objekt/{objektId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCjenici(int objektId)
        {
            var cjenici = await _context.Cjenici
                .Where(c => c.ObjektID == objektId)
                .Include(c => c.Objekt)
                .Include(c => c.CjenikArtikli!)
                    .ThenInclude(ca => ca.Artikl) 
                .OrderByDescending(c => c.DatumKreiranja)
                .Select(c => new CjenikDTO
                {
                    ID = c.ID,
                    Naziv = c.Naziv,
                    Status = c.Status,
                    DatumKreiranja = c.DatumKreiranja,
                    DatumPotvrde = c.DatumPotvrde,
                    ObjektID = c.ObjektID,
                    ObjektNaziv = c.Objekt != null ? c.Objekt.Naziv : null,
                    BrojArtikala = c.CjenikArtikli != null ? c.CjenikArtikli.Count : 0,
                    Artikli = c.CjenikArtikli != null
                        ? c.CjenikArtikli.Select(ca => new CjenikArtiklDTO
                        {
                            ArtiklID = ca.ArtiklID,
                            ArtiklNaziv = ca.Artikl != null ? ca.Artikl.Naziv : null,
                            Cijena = ca.Cijena,
                            RedoslijedPrikaza = ca.RedoslijedPrikaza,
                            Zakljucan = ca.Artikl != null ? ca.Artikl.Zakljucan : false
                        }).ToList()
                        : null
                })
                .ToListAsync();

            return Ok(cjenici);
        }

        // GET: api/cjenici/aktivni/{objektId}
        [HttpGet("aktivni/{objektId}")]
        public async Task<IActionResult> GetAktivniCjenik(int objektId)
        {
            var cjenik = await _context.Cjenici
                .Include(c => c.Objekt)
                .Include(c => c.CjenikArtikli!)
                    .ThenInclude(ca => ca.Artikl)
                .FirstOrDefaultAsync(c => c.ObjektID == objektId && c.Status == "aktivan");

            if (cjenik == null)
                return NotFound("Nema aktivnog cjenika");

            var dto = new CjenikDTO
            {
                ID = cjenik.ID,
                Naziv = cjenik.Naziv,
                Status = cjenik.Status,
                DatumKreiranja = cjenik.DatumKreiranja,
                DatumPotvrde = cjenik.DatumPotvrde,
                ObjektID = cjenik.ObjektID,
                ObjektNaziv = cjenik.Objekt?.Naziv,
                BrojArtikala = cjenik.CjenikArtikli?.Count ?? 0,
                Artikli = cjenik.CjenikArtikli?
                    .OrderBy(ca => ca.RedoslijedPrikaza)
                    .Select(ca => new CjenikArtiklDTO
                    {
                        ArtiklID = ca.ArtiklID,
                        ArtiklNaziv = ca.Artikl?.Naziv,
                        Cijena = ca.Cijena,
                        RedoslijedPrikaza = ca.RedoslijedPrikaza,
                        Zakljucan = ca.Artikl?.Zakljucan ?? false
                    }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/cjenici
        [HttpPost]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> CreateCjenik(CjenikCreateUpdateDTO dto)
        {
            // Provjeri postoji li aktivni
            var aktivni = await _context.Cjenici
                .FirstOrDefaultAsync(c => c.ObjektID == dto.ObjektID && c.Status == "aktivan");

            var cjenik = new Cjenik
            {
                Naziv = dto.Naziv ?? $"Cjenik {DateTime.Now:dd.MM.yyyy}",
                ObjektID = dto.ObjektID,
                Status = aktivni == null ? "aktivan" : "u pripremi",
                DatumKreiranja = DateTime.UtcNow
            };

            _context.Cjenici.Add(cjenik);
            await _context.SaveChangesAsync();

            // Dodaj artikle
            foreach (var a in dto.Artikli)
            {
                _context.CjenikArtikli.Add(new CjenikArtikl
                {
                    CjenikID = cjenik.ID,
                    ArtiklID = a.ArtiklID,
                    Cijena = a.Cijena,
                    RedoslijedPrikaza = a.RedoslijedPrikaza
                });
            }
            await _context.SaveChangesAsync();

            return Ok(new { id = cjenik.ID, status = cjenik.Status });
        }

        // PUT: api/cjenici/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> UpdateCjenik(int id, CjenikCreateUpdateDTO dto)
        {
            // Samo dohvati cjenik i njegove artikle, ne treba nam ThenInclude
            var cjenik = await _context.Cjenici
                .Include(c => c.CjenikArtikli)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (cjenik == null)
                return NotFound();

            if (cjenik.Status != "u pripremi")
                return BadRequest("Samo cjenici u pripremi se mogu uređivati");

            // Za ugostitelja - ne diraj zaključane artikle!
            if (User.IsInRole("Ugostitelj"))
            {
                // Trebamo dohvatiti ID-ove zaključanih artikala
                var zakljucaniArtikliIds = await _context.Artikli
                    .Where(a => a.Zakljucan)
                    .Select(a => a.ID)
                    .ToListAsync();

                // Pronađi zaključane artikle u cjeniku
                var zakljucaniUCjeniku = cjenik.CjenikArtikli?
                    .Where(ca => zakljucaniArtikliIds.Contains(ca.ArtiklID))
                    .ToList() ?? new List<CjenikArtikl>();

                // Obriši samo one koji NISU zaključani
                var zaBrisanje = cjenik.CjenikArtikli?
                    .Where(ca => !zakljucaniArtikliIds.Contains(ca.ArtiklID))
                    .ToList();

                if (zaBrisanje != null && zaBrisanje.Any())
                    _context.CjenikArtikli.RemoveRange(zaBrisanje);

                // Dodaj nove (samo slobodne)
                foreach (var a in dto.Artikli)
                {
                    // Provjeri je li zaključan
                    var jeZakljucan = await _context.Artikli
                        .Where(art => art.ID == a.ArtiklID && art.Zakljucan)
                        .AnyAsync();

                    if (jeZakljucan)
                        continue; // Preskoči zaključane

                    _context.CjenikArtikli.Add(new CjenikArtikl
                    {
                        CjenikID = id,
                        ArtiklID = a.ArtiklID,
                        Cijena = a.Cijena,
                        RedoslijedPrikaza = a.RedoslijedPrikaza
                    });
                }
            }
            else
            {
                // Administrator - može sve (briše sve)
                _context.CjenikArtikli.RemoveRange(cjenik.CjenikArtikli!);

                foreach (var a in dto.Artikli)
                {
                    _context.CjenikArtikli.Add(new CjenikArtikl
                    {
                        CjenikID = id,
                        ArtiklID = a.ArtiklID,
                        Cijena = a.Cijena,
                        RedoslijedPrikaza = a.RedoslijedPrikaza
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Naziv))
                cjenik.Naziv = dto.Naziv;

            await _context.SaveChangesAsync();
            return Ok("Cjenik ažuriran");
        }

        // POST: api/cjenici/{id}/posalji
        [HttpPost("{id}/posalji")]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> PosaljiNaPotvrdu(int id)
        {
            var cjenik = await _context.Cjenici.FindAsync(id);
            if (cjenik == null)
                return NotFound();

            if (cjenik.Status != "u pripremi")
                return BadRequest("Cjenik nije u pripremi");

            cjenik.Status = "na potvrdi";
            await _context.SaveChangesAsync();

            return Ok("Cjenik poslan na potvrdu");
        }

        // POST: api/cjenici/{id}/potvrdi
        [HttpPost("{id}/potvrdi")]
        [Authorize(Roles = "Administrator,Putnik")]
        public async Task<IActionResult> PotvrdiCjenik(int id)
        {
            var cjenik = await _context.Cjenici
                .Include(c => c.Objekt)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (cjenik == null)
                return NotFound();

            if (cjenik.Status != "na potvrdi")
                return BadRequest("Cjenik nije na potvrdi");

            // Arhiviraj stari aktivni
            var stari = await _context.Cjenici
                .FirstOrDefaultAsync(c => c.ObjektID == cjenik.ObjektID && c.Status == "aktivan");

            if (stari != null)
                stari.Status = "arhiviran";

            // Aktiviraj novi
            cjenik.Status = "aktivan";
            cjenik.DatumPotvrde = DateTime.UtcNow;
            cjenik.PotvrdioPutnikID = GetCurrentUserId();

            await _context.SaveChangesAsync();
            return Ok("Cjenik potvrđen");
        }

        // POST: api/cjenici/{id}/odbij
        [HttpPost("{id}/odbij")]
        [Authorize(Roles = "Administrator,Putnik")]
        public async Task<IActionResult> OdbijCjenik(int id)
        {
            var cjenik = await _context.Cjenici
                .Include(c => c.Objekt)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (cjenik == null)
                return NotFound("Cjenik nije pronađen.");

            if (cjenik.Status != "na potvrdi")
                return BadRequest("Samo cjenici na potvrdi se mogu odbiti.");

            // Vrati status u "u pripremi" (ugostitelj može nastaviti uređivati)
            cjenik.Status = "u pripremi";


            await _context.SaveChangesAsync();

            return Ok("Cjenik je odbijen i vraćen u pripremu.");
        }

        // GET: api/cjenici/{id}/povijest
        [HttpGet("{id}/povijest")]
        public async Task<IActionResult> GetPovijest(int id)
        {
            var cjenik = await _context.Cjenici.FindAsync(id);
            if (cjenik == null)
                return NotFound();

            var povijest = await _context.Cjenici
                .Where(c => c.ObjektID == cjenik.ObjektID && c.Status == "arhiviran")
                .OrderByDescending(c => c.DatumKreiranja)
                .Select(c => new
                {
                    c.ID,
                    c.Naziv,
                    c.Status,
                    c.DatumKreiranja,
                    c.DatumPotvrde,
                    BrojArtikala = _context.CjenikArtikli.Count(ca => ca.CjenikID == c.ID)
                })
                .ToListAsync();

            return Ok(povijest);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}

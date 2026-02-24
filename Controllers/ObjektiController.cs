using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QRCoder;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/objekti")]
    [Authorize(Roles = "Administrator,Putnik,Ugostitelj")]
    public class ObjektiController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;
        private readonly ILogger<ObjektiController> _logger;

        public ObjektiController(DigitalniCjenikContext context, ILogger<ObjektiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/objekti
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ObjektReadDTO>>> GetAll()
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                var query = _context.Objekti.AsQueryable();

                // Filtriranje prema ulogama
                if (role == "Putnik")
                {
                    query = query.Where(o => o.PutnikID == userId);
                }
                else if (role == "Ugostitelj")
                {
                    var ugostitelj = await _context.Ugostitelji
                        .FirstOrDefaultAsync(u => u.KorisnikID == userId);

                    if (ugostitelj == null)
                    {
                        _logger.LogWarning("Ugostitelj nije pronađen za korisnika {UserId}", userId);
                        return Ok(new List<ObjektReadDTO>());
                    }

                    query = query.Where(o => o.UgostiteljID == ugostitelj.ID);
                }

                // LEFT JOIN - koristimo Select umjesto Include
                var objekti = await query
                    .Select(o => new ObjektReadDTO
                    {
                        ID = o.ID,
                        Naziv = o.Naziv,
                        Adresa = o.Adresa,
                        QRKod = o.QRKod,
                        Aktivnost = o.Aktivnost,
                        UgostiteljID = o.UgostiteljID,
                        UgostiteljNaziv = o.Ugostitelj != null ? o.Ugostitelj.Naziv : null,
                        PutnikID = o.PutnikID,
                        PutnikImePrezime = o.Putnik != null ? o.Putnik.ImePrezime : null,
                        QRKodBase64 = o.QRKod != null ? GenerateQrCodeBase64(o.QRKod) : null
                    })
                    .ToListAsync();

                return Ok(objekti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatu objekata");
                return StatusCode(500, "Došlo je do greške pri dohvatu podataka");
            }
        }

        // GET: api/objekti/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ObjektReadDTO>> GetById(int id)
        {
            try
            {
                var objekt = await _context.Objekti
                    .Where(o => o.ID == id)
                    .Select(o => new ObjektReadDTO
                    {
                        ID = o.ID,
                        Naziv = o.Naziv,
                        Adresa = o.Adresa,
                        QRKod = o.QRKod,
                        Aktivnost = o.Aktivnost,
                        UgostiteljID = o.UgostiteljID,
                        UgostiteljNaziv = o.Ugostitelj != null ? o.Ugostitelj.Naziv : null,
                        PutnikID = o.PutnikID,
                        PutnikImePrezime = o.Putnik != null ? o.Putnik.ImePrezime : null,
                        QRKodBase64 = o.QRKod != null ? GenerateQrCodeBase64(o.QRKod) : null
                    })
                    .FirstOrDefaultAsync();

                if (objekt == null)
                    return NotFound("Objekt nije pronađen");

                // Provjera autorizacije
                var role = User.FindFirstValue(ClaimTypes.Role);
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (role == "Putnik" && objekt.PutnikID != userId)
                    return Forbid("Nemate pravo pristupa ovom objektu");

                if (role == "Ugostitelj")
                {
                    var ugostitelj = await _context.Ugostitelji
                        .FirstOrDefaultAsync(u => u.KorisnikID == userId);

                    if (ugostitelj == null || ugostitelj.ID != objekt.UgostiteljID)
                        return Forbid("Nemate pravo pristupa ovom objektu");
                }

                return Ok(objekt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatu objekta {Id}", id);
                return StatusCode(500, "Došlo je do greške pri dohvatu podataka");
            }
        }

        // POST: api/objekti
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ObjektReadDTO>> Create(ObjektCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Provjera Ugostitelja
                var ugostitelj = await _context.Ugostitelji.FindAsync(dto.UgostiteljID);
                if (ugostitelj == null)
                    return BadRequest($"Ugostitelj sa ID {dto.UgostiteljID} ne postoji");

                // Provjera Putnika ako je proslijeđen
                Korisnik? putnik = null;
                if (dto.PutnikID.HasValue)
                {
                    putnik = await _context.Korisnici
                        .Include(k => k.Uloga)
                        .FirstOrDefaultAsync(k => k.ID == dto.PutnikID);

                    if (putnik == null || putnik.Uloga == null || putnik.Uloga.Naziv != "Putnik")
                        return BadRequest($"Putnik sa ID {dto.PutnikID} ne postoji ili nije putnik");
                }

                // Provjera duplikata naziva za istog ugostitelja
                var postojiLi = await _context.Objekti
                    .AnyAsync(o => o.Naziv == dto.Naziv && o.UgostiteljID == dto.UgostiteljID);

                if (postojiLi)
                    return Conflict($"Objekt s nazivom '{dto.Naziv}' već postoji za ovog ugostitelja");

                // Generiranje jedinstvenog QR koda
                var qrText = $"OBJ-{Guid.NewGuid():N}";

                var objekt = new Objekt
                {
                    Naziv = dto.Naziv,
                    Adresa = dto.Adresa,
                    UgostiteljID = dto.UgostiteljID,
                    PutnikID = dto.PutnikID,
                    Aktivnost = true,
                    QRKod = qrText
                };

                _context.Objekti.Add(objekt);
                await _context.SaveChangesAsync();

                var qrKod = new QRKod
                {
                    Kod = qrText,
                    ObjektID = objekt.ID,
                    DatumGeneriranja = DateTime.UtcNow,
                    BrojSkeniranja = 0
                };
                _context.QRKod.Add(qrKod);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Kreiran novi objekt {Naziv} za ugostitelja {UgostiteljID}",
                    objekt.Naziv, objekt.UgostiteljID);

                var result = new ObjektReadDTO
                {
                    ID = objekt.ID,
                    Naziv = objekt.Naziv,
                    Adresa = objekt.Adresa,
                    QRKod = objekt.QRKod,
                    Aktivnost = objekt.Aktivnost,
                    UgostiteljID = objekt.UgostiteljID,
                    UgostiteljNaziv = ugostitelj.Naziv,
                    PutnikID = objekt.PutnikID,
                    PutnikImePrezime = putnik?.ImePrezime,
                    QRKodBase64 = GenerateQrCodeBase64(objekt.QRKod)
                };

                return CreatedAtAction(nameof(GetById), new { id = objekt.ID }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju objekta");
                return StatusCode(500, "Došlo je do greške pri kreiranju objekta");
            }
        }

        // PUT: api/objekti/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, ObjektUpdateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var objekt = await _context.Objekti.FindAsync(id);
                if (objekt == null)
                    return NotFound("Objekt nije pronađen");

                if (dto.UgostiteljID != objekt.UgostiteljID)
                {
                    var ugostitelj = await _context.Ugostitelji.FindAsync(dto.UgostiteljID);
                    if (ugostitelj == null)
                        return BadRequest($"Ugostitelj sa ID {dto.UgostiteljID} ne postoji");
                }

                if (dto.PutnikID.HasValue && dto.PutnikID != objekt.PutnikID)
                {
                    var putnik = await _context.Korisnici
                        .Include(k => k.Uloga)
                        .FirstOrDefaultAsync(k => k.ID == dto.PutnikID);

                    if (putnik == null)
                        return BadRequest($"Putnik sa ID {dto.PutnikID} ne postoji ili nije putnik");
                }

                if (dto.Naziv != objekt.Naziv)
                {
                    var postojiLi = await _context.Objekti
                        .AnyAsync(o => o.Naziv == dto.Naziv
                                    && o.UgostiteljID == dto.UgostiteljID
                                    && o.ID != id);

                    if (postojiLi)
                        return Conflict($"Objekt s nazivom '{dto.Naziv}' već postoji za ovog ugostitelja");
                }

                objekt.Naziv = dto.Naziv;
                objekt.Adresa = dto.Adresa;
                objekt.Aktivnost = dto.Aktivnost;
                objekt.UgostiteljID = dto.UgostiteljID;
                objekt.PutnikID = dto.PutnikID;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Ažuriran objekt {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju objekta {Id}", id);
                return StatusCode(500, "Došlo je do greške pri ažuriranju objekta");
            }
        }

        // DELETE: api/objekti/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var objekt = await _context.Objekti
                    .Include(o => o.Cjenici)
                    .Include(o => o.Akcije)
                    .Include(o => o.Banneri)
                    .Include(o => o.QRKodovi)
                    .Include(o => o.Datoteke)
                    .FirstOrDefaultAsync(o => o.ID == id);

                if (objekt == null)
                    return NotFound("Objekt nije pronađen");

                // Provjera da li objekt ima povezane podatke
                if (objekt.Cjenici != null && objekt.Cjenici.Any())
                    return BadRequest("Objekt se ne može obrisati jer ima povezane cjenike");

                if (objekt.Akcije != null && objekt.Akcije.Any())
                    return BadRequest("Objekt se ne može obrisati jer ima povezane akcije");

                if (objekt.Banneri != null && objekt.Banneri.Any())
                    return BadRequest("Objekt se ne može obrisati jer ima povezane banner-e");

                _context.Objekti.Remove(objekt);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Obrisan objekt {Id} - {Naziv}", objekt.ID, objekt.Naziv);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brisanju objekta {Id}", id);
                return StatusCode(500, "Došlo je do greške pri brisanju objekta");
            }
        }

        // PATCH: api/objekti/{id}/aktivnost
        [HttpPatch("{id}/aktivnost")]
        [Authorize(Roles = "Administrator,Ugostitelj")]
        public async Task<IActionResult> ToggleAktivnost(int id)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                var objekt = await _context.Objekti
                    .Include(o => o.Ugostitelj)
                    .FirstOrDefaultAsync(o => o.ID == id);

                if (objekt == null)
                    return NotFound("Objekt nije pronađen");

                // Provjera autorizacije za Ugostitelja
                if (role == "Ugostitelj")
                {
                    var ugostitelj = await _context.Ugostitelji
                        .FirstOrDefaultAsync(u => u.KorisnikID == userId);

                    if (ugostitelj == null || ugostitelj.ID != objekt.UgostiteljID)
                        return Forbid("Nemate pravo mijenjati aktivnost ovog objekta");
                }

                objekt.Aktivnost = !objekt.Aktivnost;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    aktivnost = objekt.Aktivnost,
                    poruka = $"Aktivnost objekta promijenjena na {(objekt.Aktivnost ? "aktivno" : "neaktivno")}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri promjeni aktivnosti objekta {Id}", id);
                return StatusCode(500, "Došlo je do greške pri promjeni aktivnosti");
            }
        }

        // GET: api/objekti/qr/{qrKod}
        [HttpGet("qr/{qrKod}")]
        [AllowAnonymous]
        public async Task<ActionResult<ObjektReadDTO>> GetByQRCode(string qrKod)
        {
            try
            {
                var objekt = await _context.Objekti
                    .Where(o => o.QRKod == qrKod)
                    .Select(o => new ObjektReadDTO
                    {
                        ID = o.ID,
                        Naziv = o.Naziv,
                        Adresa = o.Adresa,
                        QRKod = o.QRKod,
                        Aktivnost = o.Aktivnost,
                        UgostiteljID = o.UgostiteljID,
                        UgostiteljNaziv = o.Ugostitelj != null ? o.Ugostitelj.Naziv : null,
                        PutnikID = o.PutnikID,
                        PutnikImePrezime = o.Putnik != null ? o.Putnik.ImePrezime : null,
                        QRKodBase64 = o.QRKod != null ? GenerateQrCodeBase64(o.QRKod) : null
                    })
                    .FirstOrDefaultAsync();

                if (objekt == null)
                    return NotFound("Objekt nije pronađen");

                if (!objekt.Aktivnost)
                    return BadRequest("Objekt je trenutno neaktivan");

                // Zabilježi skeniranje u analitiku
                var analitika = new Analitika
                {
                    TipDogadaja = "QR scan",
                    DatumVrijeme = DateTime.UtcNow,
                    ObjektID = objekt.ID,
                    DodatniParametri = $"QR kod: {qrKod}"
                };
                _context.Analitika.Add(analitika);

                var qrKodEntity = await _context.QRKod.FirstOrDefaultAsync(q => q.Kod == qrKod);
                if (qrKodEntity != null)
                {
                    qrKodEntity.BrojSkeniranja++;
                }
                else
                {
                    var noviQRKod = new QRKod
                    {
                        Kod = qrKod,
                        ObjektID = objekt.ID,
                        DatumGeneriranja = DateTime.UtcNow,
                        BrojSkeniranja = 1
                    };
                    _context.QRKod.Add(noviQRKod);
                }

                await _context.SaveChangesAsync();

                return Ok(objekt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatu objekta po QR kodu");
                return StatusCode(500, "Došlo je do greške pri dohvatu podataka");
            }
        }

        private static string GenerateQrCodeBase64(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using var qrGenerator = new QRCodeGenerator();
            using var data = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qr = new Base64QRCode(data);
            return qr.GetGraphic(20);
        }
    }
}
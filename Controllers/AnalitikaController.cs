using DigitalniCjenik.Data;
using DigitalniCjenik.DTO;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [ApiController]
    [Route("api/analitika")]
    public class AnalitikaController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;

        public AnalitikaController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        // POST: api/analitika/zabiljezi
        [HttpPost("zabiljezi")]
        [AllowAnonymous] // Javni endpoint za bilježenje događaja
        public async Task<IActionResult> ZabiljeziDogadaj(AnalitikaDogadajDTO dto)
        {
            var analitika = new Analitika
            {
                TipDogadaja = dto.TipDogadaja,
                ObjektID = dto.ObjektID,
                CjenikID = dto.CjenikID,
                DodatniParametri = dto.DodatniParametri,
                DatumVrijeme = DateTime.UtcNow
            };

            _context.Analitika.Add(analitika);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/analitika/izvjestaj/qr
        [HttpGet("izvjestaj/qr")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetQRIzvjestaj([FromQuery] DateTime? od = null, [FromQuery] DateTime? @do = null)
        {
            od ??= DateTime.UtcNow.AddDays(-30);
            @do ??= DateTime.UtcNow;

            var qrScanovi = await _context.Analitika
                .Include(a => a.Objekt)
                .Where(a => a.TipDogadaja == "QR scan" &&
                            a.DatumVrijeme >= od && a.DatumVrijeme <= @do)
                .GroupBy(a => a.ObjektID)
                .Select(g => new AnalitikaStavkaDTO
                {
                    Kljuc = g.Select(a => a.Objekt!.Naziv).FirstOrDefault() ?? $"Objekt {g.Key}",
                    Vrijednost = g.Count()
                })
                .ToListAsync();

            var ukupno = qrScanovi.Sum(s => s.Vrijednost);

            foreach (var s in qrScanovi)
                s.Postotak = ukupno > 0 ? Math.Round(s.Vrijednost * 100.0 / ukupno, 2) : 0;

            return Ok(new AnalitikaIzvjestajDTO
            {
                Naziv = "QR Scanovi po objektima",
                Ukupno = ukupno,
                Stavke = qrScanovi
            });
        }

        // GET: api/analitika/izvjestaj/otvaranja
        [HttpGet("izvjestaj/otvaranja")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetOtvaranjaIzvjestaj([FromQuery] DateTime? od = null, [FromQuery] DateTime? @do = null)
        {
            od ??= DateTime.UtcNow.AddDays(-30);
            @do ??= DateTime.UtcNow;

            var otvaranja = await _context.Analitika
                .Include(a => a.Objekt)
                .Where(a => a.TipDogadaja == "otvoren cjenik" &&
                           a.DatumVrijeme >= od && a.DatumVrijeme <= @do)
                .GroupBy(a => a.ObjektID)
                .Select(g => new AnalitikaStavkaDTO
                {
                    Kljuc = g.Select(a => a.Objekt!.Naziv).FirstOrDefault() ?? $"Objekt {g.Key}",
                    Vrijednost = g.Count()
                })
                .ToListAsync();

            return Ok(otvaranja);
        }

        // GET: api/analitika/izvjestaj/vremenski
        [HttpGet("izvjestaj/vremenski")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetVremenskiIzvjestaj(
            [FromQuery] string tipDogadaja = "QR scan",
            [FromQuery] int? objektId = null,
            [FromQuery] int dana = 30)
        {
            var od = DateTime.UtcNow.AddDays(-dana);

            var query = _context.Analitika
                .Where(a => a.TipDogadaja == tipDogadaja && a.DatumVrijeme >= od);

            if (objektId.HasValue)
                query = query.Where(a => a.ObjektID == objektId);

            var rezultati = await query
                .GroupBy(a => a.DatumVrijeme.Date)
                .Select(g => new AnalitikaVremenskiDTO
                {
                    Datum = g.Key,
                    TipDogadaja = tipDogadaja,
                    Broj = g.Count()
                })
                .OrderBy(r => r.Datum)
                .ToListAsync();

            return Ok(rezultati);
        }

        // GET: api/analitika/izvjestaj/klikovi
        [HttpGet("izvjestaj/klikovi")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetKlikoviIzvjestaj([FromQuery] DateTime? od = null, [FromQuery] DateTime? @do = null)
        {
            od ??= DateTime.UtcNow.AddDays(-30);
            @do ??= DateTime.UtcNow;

            var klikovi = await _context.Analitika
                .Where(a => a.TipDogadaja != null && a.TipDogadaja.StartsWith("klik") &&
                            a.DatumVrijeme >= od && a.DatumVrijeme <= @do)
                .GroupBy(a => a.TipDogadaja)
                .Select(g => new AnalitikaStavkaDTO
                {
                    Kljuc = g.Key ?? "Nepoznato",
                    Vrijednost = g.Count()
                })
                .ToListAsync();

            return Ok(klikovi);
        }

        // GET: api/analitika/dashboard
        [HttpGet("dashboard")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDashboard()
        {
            var danas = DateTime.UtcNow.Date;
            var prije7dana = danas.AddDays(-7);

            // Ukupni brojevi
            var ukupnoQr = await _context.Analitika
                .CountAsync(a => a.TipDogadaja == "QR scan" && a.DatumVrijeme >= prije7dana);

            var ukupnoOtvorenih = await _context.Analitika
                .CountAsync(a => a.TipDogadaja == "otvoren cjenik" && a.DatumVrijeme >= prije7dana);

            var ukupnoKlikova = await _context.Analitika
                .CountAsync(a => a.TipDogadaja != null && a.TipDogadaja.StartsWith("klik") && a.DatumVrijeme >= prije7dana);

            // Grupiranje po danima
            var poDanima = await _context.Analitika
                .Where(a => a.DatumVrijeme >= prije7dana)
                .GroupBy(a => a.DatumVrijeme.Date)
                .Select(g => new
                {
                    Datum = g.Key,
                    QR = g.Count(a => a.TipDogadaja == "QR scan"),
                    Otvaranja = g.Count(a => a.TipDogadaja == "otvoren cjenik"),
                    Klikovi = g.Count(a => a.TipDogadaja != null && a.TipDogadaja.StartsWith("klik"))
                })
                .OrderBy(g => g.Datum)
                .ToListAsync();

            return Ok(new
            {
                ukupnoQr,
                ukupnoOtvorenih,
                ukupnoKlikova,
                poDanima
            });
        }
    }

}

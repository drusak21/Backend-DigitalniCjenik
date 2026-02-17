using DigitalniCjenik.Data;
using DigitalniCjenik.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UlogeController : ControllerBase
    {
        private readonly DigitalniCjenikContext _context;
        public UlogeController(DigitalniCjenikContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uloga>>> GetUloge()
        {
            return await _context.Uloge.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Uloga>> PostUloga(Uloga uloga)
        {
            _context.Uloge.Add(uloga);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUloge), new { id = uloga.ID }, uloga);
        }
    }
}

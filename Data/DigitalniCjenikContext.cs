using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Data
{
    public class DigitalniCjenikContext : DbContext
    {
        public DigitalniCjenikContext(DbContextOptions<DigitalniCjenikContext> options)
            : base(options)
        {
        }

       // public DbSet<Uloga> Uloga { get; set; }
        //public DbSet<Korisnik> Korisnici { get; set; }

    }
}

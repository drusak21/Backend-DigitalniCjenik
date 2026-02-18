using System.Collections.Generic;
using DigitalniCjenik.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalniCjenik.Data
{
    public class DigitalniCjenikContext : DbContext
    {
        public DigitalniCjenikContext(DbContextOptions<DigitalniCjenikContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Korisnik>()
                .ToTable("Korisnik"); // mapira DbSet Korisnici → tablica Korisnik

            modelBuilder.Entity<Uloga>()
                .ToTable("Uloga");
        }

        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }

    }
}

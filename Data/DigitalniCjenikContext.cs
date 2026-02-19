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
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik"); // mapira DbSet Korisnici na tablica Korisnik

            modelBuilder.Entity<Uloga>().ToTable("Uloga");

            modelBuilder.Entity<Ugostitelj>().ToTable("Ugostitelj");

            modelBuilder.Entity<Objekt>().ToTable("Objekt");
            modelBuilder.Entity<Analitika>().ToTable("Analitika");
            modelBuilder.Entity<QRKod>().ToTable("QRKod");

            // Korisnik → Uloga (više korisnika, jedna uloga)
            modelBuilder.Entity<Korisnik>()
                .HasOne(k => k.Uloga)
                .WithMany(u => u.Korisnici)
                .HasForeignKey(k => k.UlogaID);

            // Ugostitelj → Korisnik (admin/ugostitelj vlasnik)
            modelBuilder.Entity<Ugostitelj>()
                .HasOne(u => u.Korisnik)
                .WithMany(k => k.Ugostitelji)
                .HasForeignKey(u => u.KorisnikID)
                .OnDelete(DeleteBehavior.Restrict);

            // Ugostitelj → Objekti (1:N)
            modelBuilder.Entity<Objekt>()
                .HasOne(o => o.Ugostitelj)
                .WithMany(u => u.Objekti)
                .HasForeignKey(o => o.UgostiteljID)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Ugostitelj> Ugostitelji { get; set; }
        public DbSet<Objekt> Objekti { get; set; }
        public DbSet<Analitika> Analitika { get; set; }
        public DbSet<QRKod> QRKod { get; set; }

    }
}

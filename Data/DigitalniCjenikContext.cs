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
            modelBuilder.Entity<Kategorija>().ToTable("Kategorija");
            modelBuilder.Entity<Artikl>().ToTable("Artikl");
            modelBuilder.Entity<Cjenik>().ToTable("Cjenik");
            modelBuilder.Entity<CjenikArtikl>().ToTable("CjenikArtikl");

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

            modelBuilder.Entity<Artikl>(entity =>
            {
                entity.HasKey(a => a.ID);

                entity.Property(a => a.Naziv)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(a => a.Cijena)
                    .HasColumnType("decimal(10,2)");

                entity.Property(a => a.Brand)
                    .HasMaxLength(50);

                entity.Property(a => a.Slika)
                    .HasMaxLength(255);

                // Relacija s kategorijom
                entity.HasOne(a => a.Kategorija)
                    .WithMany(k => k.Artikli)
                    .HasForeignKey(a => a.KategorijaID)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indeksi za bolje performanse
                entity.HasIndex(a => a.Naziv);
                entity.HasIndex(a => a.Brand);
                entity.HasIndex(a => a.Zakljucan);
            });

            
            modelBuilder.Entity<Kategorija>(entity =>
            {
                entity.HasKey(k => k.ID);

                entity.Property(k => k.Naziv)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(k => k.Naziv);
                entity.HasIndex(k => k.Aktivan);
            });

            modelBuilder.Entity<Cjenik>(entity =>
            {
                entity.HasKey(c => c.ID);
                entity.Property(c => c.Naziv).HasMaxLength(100);
                entity.Property(c => c.Status).HasMaxLength(20); // "u pripremi", "na potvrdi", "aktivan", "arhiviran"

                // Veza s Objektom
                entity.HasOne(c => c.Objekt)
                    .WithMany(o => o.Cjenici)
                    .HasForeignKey(c => c.ObjektID)
                    .OnDelete(DeleteBehavior.Cascade);

                

                // Indeksi
                entity.HasIndex(c => new { c.ObjektID, c.Status });
            });

            modelBuilder.Entity<CjenikArtikl>(entity =>
            {
                entity.HasKey(ca => ca.ID);

                // Unique constraint - jedan artikl jednom u cjeniku
                entity.HasIndex(ca => new { ca.CjenikID, ca.ArtiklID })
                    .IsUnique();

                entity.Property(ca => ca.Cijena)
                    .HasColumnType("decimal(10,2)");

                // Veza s Cjenikom
                entity.HasOne(ca => ca.Cjenik)
                    .WithMany(c => c.CjenikArtikli)
                    .HasForeignKey(ca => ca.CjenikID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Veza s Artiklom
                entity.HasOne(ca => ca.Artikl)
                    .WithMany(a => a.CjenikArtikli)
                    .HasForeignKey(ca => ca.ArtiklID)
                    .OnDelete(DeleteBehavior.Restrict); // Ne želimo obrisati artikl ako se obriše iz cjenika

                entity.HasIndex(ca => ca.RedoslijedPrikaza);
            });

        }

        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Ugostitelj> Ugostitelji { get; set; }
        public DbSet<Objekt> Objekti { get; set; }
        public DbSet<Analitika> Analitika { get; set; }
        public DbSet<QRKod> QRKod { get; set; }
        public DbSet<Kategorija> Kategorije { get; set; }
        public DbSet<Artikl> Artikli { get; set; }
        public DbSet<Cjenik> Cjenici { get; set; }
        public DbSet<CjenikArtikl> CjenikArtikli { get; set; }

    }
}

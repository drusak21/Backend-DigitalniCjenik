namespace DigitalniCjenik.DTO
{
    public class AkcijaDTO
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Opis { get; set; }
        public string? Vrsta { get; set; } 
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }
        public string? Slika { get; set; }
        public bool Aktivna { get; set; }
        public int? ObjektID { get; set; }
        public string? ObjektNaziv { get; set; }
        public bool AktivnaSada => Aktivna &&
        (!DatumPocetka.HasValue || DatumPocetka.Value.ToUniversalTime() <= DateTime.UtcNow) &&
        (!DatumZavrsetka.HasValue || DatumZavrsetka.Value.ToUniversalTime() >= DateTime.UtcNow);
    }
}

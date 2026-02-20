namespace DigitalniCjenik.DTO
{
    public class AkcijaCreateDTO
    {
        public string? Naziv { get; set; }
        public string? Opis { get; set; }
        public string? Vrsta { get; set; }
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }
        public string? Slika { get; set; }
        public int? ObjektID { get; set; }
        public int? ArtiklID { get; set; }
    }
}

namespace DigitalniCjenik.Models
{
    public class Cjenik
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public int ObjektID { get; set; }
        public Objekt? Objekt { get; set; }
        public string? Status { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public DateTime? DatumPotvrde { get; set; }

        public int? PotvrdioPutnikID { get; set; }
        public Korisnik? PotvrdioPutnik { get; set; }

        public ICollection<CjenikArtikl>? CjenikArtikli { get; set; }
        public ICollection<Analitika>? Analitika { get; set; }
    }
}

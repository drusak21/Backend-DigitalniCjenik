namespace DigitalniCjenik.Models
{
    public class Objekt
    {
        public int ID { get; set; }
        public string ?Naziv { get; set; }
        public string ?Adresa { get; set; }
        public string ?QRKod { get; set; }
        public bool Aktivnost { get; set; }

        public int UgostiteljID { get; set; }
        public Ugostitelj ?Ugostitelj { get; set; }

        public int? PutnikID { get; set; }
        public Korisnik ?Putnik { get; set; }

        public ICollection<Cjenik>? Cjenici { get; set; }
        public ICollection<Akcija>? Akcije { get; set; }
        public ICollection<Banner>? Banneri { get; set; }
        public ICollection<QRKod>? QRKodovi { get; set; }
        public ICollection<Analitika>? Analitika { get; set; }
        public ICollection<Datoteka>? Datoteke { get; set; }
    }
}
    

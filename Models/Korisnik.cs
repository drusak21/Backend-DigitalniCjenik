namespace DigitalniCjenik.Models
{
    public class Korisnik
    {
        public int ID { get; set; }
        public string ?ImePrezime { get; set; }
        public string ?Email { get; set; }
        public byte[]? LozinkaHash { get; set; }
        public byte[]? LozinkaSalt { get; set; }
        public int UlogaID { get; set; }
        public Uloga ?Uloga { get; set; }
        public string JezikSučelja { get; set; } = "HR";
        public bool Aktivnost { get; set; } = true;

        public ICollection<Ugostitelj>? Ugostitelji { get; set; }
        public ICollection<Objekt>? Objekti { get; set; } // PutnikID
        public ICollection<Cjenik>? PotvrdjeniCjenici { get; set; }
        public ICollection<Datoteka>? Datoteke { get; set; }
    }
}

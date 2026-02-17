namespace DigitalniCjenik.Models
{
    public class Ugostitelj
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? KontaktEmail { get; set; }
        public string? KontaktTelefon { get; set; }
        public string? OIB { get; set; }
        public string? Logotip { get; set; }
        public string? BrandingBoje { get; set; }

        public int KorisnikID { get; set; }
        public Korisnik? Korisnik { get; set; }

        public ICollection<Objekt>? Objekti { get; set; }
    }
}

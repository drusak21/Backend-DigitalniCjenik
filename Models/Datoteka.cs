namespace DigitalniCjenik.Models
{
    public class Datoteka
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Tip { get; set; }
        public int? Velicina { get; set; }
        public byte[]? Sadrzaj { get; set; }
        public DateTime DatumUcitanja { get; set; }
        public string? StatusObrade { get; set; }

        public int? ObjektID { get; set; }
        public Objekt? Objekt { get; set; }

        public int? KorisnikID { get; set; }
        public Korisnik? Banneri { get; set; }
    }
}

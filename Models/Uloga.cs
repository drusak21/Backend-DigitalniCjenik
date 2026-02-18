using System.Text.Json.Serialization;

namespace DigitalniCjenik.Models
{
    public class Uloga
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? OpisPrava { get; set; }

        public ICollection<Korisnik>? Korisnici { get; set; }
    }
}

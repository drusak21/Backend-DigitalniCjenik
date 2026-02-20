using System.Reflection;
using System.Collections.Generic;

namespace DigitalniCjenik.Models
{
    public class Akcija
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Opis { get; set; }
        public string? Vrsta { get; set; }
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }
        public string? Slika { get; set; }
        public bool Aktivna { get; set; } = true;

        public int? ObjektID { get; set; }
        public Objekt? Objekt { get; set; }

        public int? ArtiklID { get; set; }  
        public Artikl? Artikl { get; set; }

        public ICollection<Banner>? Banneri { get; set; }
    }
}

namespace DigitalniCjenik.Models
{
    public class CjenikArtikl
    {
        public int ID { get; set; }
        public int CjenikID { get; set; }
        public Cjenik? Cjenik { get; set; }

        public int ArtiklID { get; set; }
        public Artikl? Artikl { get; set; }

        public decimal Cijena { get; set; }
        public int RedoslijedPrikaza { get; set; } = 1;
        public bool Aktivan { get; set; } = true;
    }
}

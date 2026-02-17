namespace DigitalniCjenik.Models
{
    public class Kategorija
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public int RedoslijedPrikaza { get; set; } = 1;
        public bool Aktivan { get; set; } = true;

        public ICollection<Artikl>? Artikli { get; set; }
    }
}

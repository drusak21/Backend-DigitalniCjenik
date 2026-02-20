namespace DigitalniCjenik.DTO
{
    public class CjenikStavkaDTO
    {
        public int ArtiklID { get; set; }
        public decimal Cijena { get; set; }
        public int RedoslijedPrikaza { get; set; } = 1;
    }
}

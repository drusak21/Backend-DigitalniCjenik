namespace DigitalniCjenik.DTO
{
    public class CjenikArtiklDTO
    {
        public int ArtiklID { get; set; }
        public string? ArtiklNaziv { get; set; }
        public decimal Cijena { get; set; }
        public int RedoslijedPrikaza { get; set; }
        public bool Zakljucan { get; set; }
    }
}

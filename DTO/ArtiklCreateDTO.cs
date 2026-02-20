namespace DigitalniCjenik.DTO
{
    public class ArtiklCreateDTO
    {
        public string Naziv { get; set; } = null!;
        public string? Opis { get; set; }
        public decimal Cijena { get; set; }
        public string? SastavAlergeni { get; set; }
        public string? Slika { get; set; }
        public string? Brand { get; set; }
        public int? KategorijaID { get; set; }
        public bool Zakljucan { get; set; } = false;
    }
}

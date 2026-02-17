namespace DigitalniCjenik.Models
{
    public class Artikl
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Opis { get; set; }
        public decimal Cijena { get; set; }
        public string? SastavAlergeni { get; set; }
        public string? Slika { get; set; }
        public string? Brand { get; set; }
        public bool Zakljucan { get; set; } = false;

        public int? KategorijaID { get; set; }
        public Kategorija? Kategorija { get; set; }

        public ICollection<CjenikArtikl>? CjenikArtikli { get; set; }
    }
}

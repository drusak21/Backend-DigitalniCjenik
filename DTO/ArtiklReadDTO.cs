namespace DigitalniCjenik.DTO
{
    public class ArtiklReadDTO
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Opis { get; set; }
        public decimal Cijena { get; set; }
        public string? SastavAlergeni { get; set; }
        public string? Slika { get; set; }
        public string? Brand { get; set; }
        public bool Zakljucan { get; set; }

        public int? KategorijaID { get; set; }
        public string? KategorijaNaziv { get; set; }
    }
}

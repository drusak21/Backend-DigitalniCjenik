namespace DigitalniCjenik.DTO
{
    public class AnalitikaIzvjestajDTO
    {
        public string? Naziv { get; set; }
        public int Ukupno { get; set; }
        public List<AnalitikaStavkaDTO>? Stavke { get; set; }
    }
}

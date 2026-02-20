namespace DigitalniCjenik.DTO
{
    public class KategorijaReadDTO
    {
        public int ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public int RedoslijedPrikaza { get; set; }
        public bool Aktivan { get; set; }
    }
}

namespace DigitalniCjenik.DTO
{
    public class KategorijaUpdateDTO
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public int? RedoslijedPrikaza { get; set; }
        public bool? Aktivan { get; set; }
    }
}

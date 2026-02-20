namespace DigitalniCjenik.Models
{
    public class Analitika
    {
        public int ID { get; set; }
        public string? TipDogadaja { get; set; }
        public DateTime DatumVrijeme { get; set; }
        public string? DodatniParametri { get; set; }

        public int? ObjektID { get; set; }
        public Objekt? Objekt { get; set; }

        public int? CjenikID { get; set; }
        public Cjenik? Cjenik { get; set; }
    }
}

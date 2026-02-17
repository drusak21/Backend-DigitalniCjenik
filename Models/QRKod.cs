namespace DigitalniCjenik.Models
{
    public class QRKod
    {
        public int ID { get; set; }
        public string? Kod { get; set; }
        public DateTime DatumGeneriranja { get; set; }
        public int BrojSkeniranja { get; set; } = 0;

        public int ObjektID { get; set; }
        public Objekt? Objekt { get; set; }
    }
}

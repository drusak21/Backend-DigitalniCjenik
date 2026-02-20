namespace DigitalniCjenik.Models
{
    public class Banner
    {
        public int ID { get; set; }
        public string? Tip { get; set; }
        public string? Sadrzaj { get; set; }
        public bool Aktivan { get; set; } = true;

        public int? ObjektID { get; set; }
        public Objekt? Objekt { get; set; }

        public int? AkcijaID { get; set; }
        public Akcija? Akcija { get; set; }
    }
}

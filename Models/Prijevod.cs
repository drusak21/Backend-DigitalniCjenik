namespace DigitalniCjenik.Models
{
    public class Prijevod
    {
        public int ID { get; set; }
        public int JezikID { get; set; }
        public Jezik? Banneri { get; set; }
        public string? Sadrzaj { get; set; }
    }
}

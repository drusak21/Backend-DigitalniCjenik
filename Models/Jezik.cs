namespace DigitalniCjenik.Models
{
    public class Jezik
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? KodJezika { get; set; }

        public ICollection<Prijevod>? Prijevodi { get; set; }
    }
}

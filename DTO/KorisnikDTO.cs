namespace DigitalniCjenik.DTO
{
    public class KorisnikDTO
    {
        public int ID { get; set; }
        public string ImePrezime { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UlogaNaziv { get; set; } = null!;
        public string OpisUloge { get; set; } = null!;
        public string JezikSučelja { get; set; } = "HR";
        public bool Aktivnost { get; set; }
    }
}

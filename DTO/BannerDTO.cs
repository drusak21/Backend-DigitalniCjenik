namespace DigitalniCjenik.DTO
{
    public class BannerDTO
    {
        public int ID { get; set; }
        public string? Tip { get; set; } // "homepage", "kategorija", "popup"
        public string? Sadrzaj { get; set; }
        public bool Aktivan { get; set; }
        public int? ObjektID { get; set; }
        public string? ObjektNaziv { get; set; }
        public int? AkcijaID { get; set; }
        public string? AkcijaNaziv { get; set; }
    }
}

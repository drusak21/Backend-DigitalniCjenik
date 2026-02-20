namespace DigitalniCjenik.DTO
{
    public class CjenikDTO
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Status { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public DateTime? DatumPotvrde { get; set; }
        public int ObjektID { get; set; }
        public string? ObjektNaziv { get; set; }
        public int BrojArtikala { get; set; }
        public List<CjenikArtiklDTO>? Artikli { get; set; }
    }
}

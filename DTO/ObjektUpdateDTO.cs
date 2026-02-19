namespace DigitalniCjenik.DTO
{
    public class ObjektUpdateDTO
    {
        public string? Naziv { get; set; }
        public string? Adresa { get; set; }
        public bool Aktivnost { get; set; }
        public int UgostiteljID { get; set; }
        public int? PutnikID { get; set; }
    }
}

namespace DigitalniCjenik.DTO
{
    public class ObjektReadDTO
    {
        public int ID { get; set; }
        public string? Naziv { get; set; }
        public string? Adresa { get; set; }
        public string? QRKod { get; set; } 
        public bool Aktivnost { get; set; }
        public int UgostiteljID { get; set; }
        public string? UgostiteljNaziv { get; set; }
        public int? PutnikID { get; set; }
        public string? PutnikImePrezime { get; set; }
        public string? QRKodBase64 { get; set; } 
    }
}

namespace DigitalniCjenik.DTO
{
    public class AnalitikaDogadajDTO
    {
        public string? TipDogadaja { get; set; } // "QR scan", "otvoren cjenik", "klik banner", "pregled kategorije"
        public int? ObjektID { get; set; }
        public int? CjenikID { get; set; }
        public string? DodatniParametri { get; set; } // JSON za ID bannera, artikla itd.
    }
}

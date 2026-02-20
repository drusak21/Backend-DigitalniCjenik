namespace DigitalniCjenik.DTO
{
    public class CjenikCreateUpdateDTO
    {
        public string? Naziv { get; set; }
        public int ObjektID { get; set; }
        public List<CjenikStavkaDTO> Artikli { get; set; } = new();
    }
}

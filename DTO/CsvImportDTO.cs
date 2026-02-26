using Microsoft.AspNetCore.Mvc;

namespace DigitalniCjenik.DTO
{
    public class CsvImportDTO
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = null!;
    }
}

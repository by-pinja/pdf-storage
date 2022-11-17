using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Pdf.Storage.Test.Dto
{
    public class NewPdfRequestTest
    {
        [Required]
        public string Html { get; set; }

        [Required]
        public JsonDocument BaseData { get; set; } = JsonDocument.Parse("{}");

        [Required]
        public JsonDocument[] RowData { get; set; } = Array.Empty<JsonDocument>();
        public JsonDocument Options { get; set; } = JsonDocument.Parse("{}");
    }
}

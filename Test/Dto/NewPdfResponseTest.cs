using System.Text.Json;

namespace Pdf.Storage.Test.Dto
{
    public class NewPdfResponseTest
    {
        public string PdfUri { get; set; }
        public string HtmlUri { get; set; }
        public string Id { get; set; }
        public string GroupId { get; set; }
        public JsonDocument Data { get; set; }
    }
}

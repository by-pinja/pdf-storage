using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf.Dto
{
    public class NewPdfResponse
    {
        public NewPdfResponse(string id, string groupId, string pdfUri, string htmlUri, JObject data)
        {
            Id = id;
            GroupId = groupId;
            PdfUri = pdfUri;
            Data = data;
            HtmlUri = htmlUri;
        }

        public string PdfUri { get; }
        public string HtmlUri { get; }
        public string Id { get; }
        public string GroupId { get; }
        public JObject Data { get; }
    }
}
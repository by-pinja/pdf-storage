namespace Pdf.Storage.Pdf.Dto
{
    public class NewPdfResponse
    {
        public NewPdfResponse(string id, string groupId, string pdfUri, object data)
        {
            Id = id;
            GroupId = groupId;
            PdfUri = pdfUri;
            Data = data;
        }

        public string PdfUri { get; }
        public string HtmlUri { get; }
        public string Id { get; }
        public string GroupId { get; }
        public object Data { get; }
    }
}
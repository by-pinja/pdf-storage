namespace Pdf.Storage.PdfMerge
{
    public class MergeResponse
    {
        public MergeResponse(string id, string pdfUri)
        {
            Id = id;
            PdfUri = pdfUri;
        }

        public string Id { get; }

        public string PdfUri { get; }
    }
}

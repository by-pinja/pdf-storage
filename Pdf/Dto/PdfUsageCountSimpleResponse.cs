namespace Pdf.Storage.Pdf
{
    public class PdfUsageCountSimpleResponse
    {
        public PdfUsageCountSimpleResponse(string pdfId, bool isOpened)
        {
            PdfId = pdfId;
            IsOpened = isOpened;
        }

        public string PdfId { get; set; }
        public bool IsOpened { get; }
    }
}
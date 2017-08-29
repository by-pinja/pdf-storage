namespace Pdf.Storage.Mq
{
    public interface IMqMessages
    {
        void PdfGenerated(string groupId, string pdfId);
        void PdfOpened(string groupId, string pdfId);
    }
}
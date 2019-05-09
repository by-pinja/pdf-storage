namespace Pdf.Storage.Pdf
{
    public interface IStorage
    {
        void AddOrReplacePdf(StoredPdf pdf);
        void RemovePdf(string groupId, string pdfId);
        StoredPdf GetPdf(string groupId, string pdfId);
    }
}
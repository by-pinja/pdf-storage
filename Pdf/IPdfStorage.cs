namespace Pdf.Storage.Pdf
{
    public interface IPdfStorage
    {
        void AddOrReplacePdf(StoredPdf pdf);
        StoredPdf GetPdf(string groupId, string fileId);
    }
}
using Pdf.Storage.Pdf;

namespace Pdf.Storage.Test
{
    public interface IPdfStorage
    {
        void AddPdf(StoredPdf pdf);
        StoredPdf GetPdf(string groupId, string fileId);
    }
}
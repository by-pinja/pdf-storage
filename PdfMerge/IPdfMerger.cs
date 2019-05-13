using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.PdfMerge
{
    public interface IPdfMerger
    {
        void MergePdf(StorageFileId storageIdForMergedPdf, string[] requests);
    }
}
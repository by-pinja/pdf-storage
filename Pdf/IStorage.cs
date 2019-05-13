using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Pdf
{
    public interface IStorage
    {
        void AddOrReplace(StorageData pdf);
        void Remove(StorageFileId storageFileId);
        StorageData Get(StorageFileId storageFileId);
    }
}
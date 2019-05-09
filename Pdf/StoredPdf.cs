using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Pdf
{
    public class StorageData
    {
        public StorageFileId StorageFileId { get; }
        public byte[] Data { get; }

        public StorageData(StorageFileId storageFileId, byte[] data)
        {
            Data = data;
            StorageFileId = storageFileId;
        }
    }
}

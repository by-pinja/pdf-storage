using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Pdf
{
    public class StorageData
    {
        public StorageFileId StorageFileId { get; }

        public string ContentType { get; }

        public byte[] Data { get; }

        public StorageData(StorageFileId storageFileId, byte[] data)
        {
            Data = data;
            StorageFileId = storageFileId;

            ContentType = storageFileId.Extension switch
            {
                "html" => "text/html",
                "pdf" => "application/pdf",
                _ => "text/plain",
            };
        }
    }
}

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

            switch(storageFileId.Extension)
            {
                case "html":
                    ContentType = "text/html";
                    break;
                case "pdf":
                    ContentType = "application/pdf";
                    break;
                default:
                    ContentType = "text/plain";
                    break;
            }
        }
    }
}

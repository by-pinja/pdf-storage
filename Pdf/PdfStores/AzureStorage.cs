using System;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using Pdf.Storage.Pdf.Config;

namespace Pdf.Storage.Pdf.PdfStores
{
    public class AzureStorage : IStorage
    {
        private readonly Lazy<CloudBlobContainer> _blobContainer;

        public AzureStorage(IOptions<AzureStorageConfig> azureConfig)
        {
            _blobContainer = new Lazy<CloudBlobContainer>(() =>
            {
                var storageAccount = CloudStorageAccount.Parse(azureConfig.Value.StorageConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(azureConfig.Value.ContainerName);
                blobContainer.CreateIfNotExists();
                return blobContainer;
            });
        }

        public void AddOrReplace(StorageData storageData)
        {
            var blob = _blobContainer.Value;
            var blobRef = GetBlobRef(storageData.StorageFileId, blob);
            blobRef.UploadFromByteArray(storageData.Data, 0, storageData.Data.Length);
        }

        private static CloudBlockBlob GetBlobRef(StorageFileId storageFileId, CloudBlobContainer blob)
        {
            return blob.GetBlockBlobReference(GetBlobName(storageFileId));
        }

        public StorageData Get(StorageFileId storageFileId)
        {
            using var memorySteam = new MemoryStream();

            var blob = _blobContainer.Value;

            var blobRef = GetBlobRef(storageFileId, blob);

            if (!blobRef.Exists())
                throw new InvalidOperationException($"Tried to open non existent blob '{GetBlobName(storageFileId)}'");

            blobRef.DownloadToStream(memorySteam);

            var asDataArray = memorySteam.ToArray();

            return new StorageData(storageFileId, asDataArray);
        }

        public void Remove(StorageFileId storageFileId)
        {
            var blob = _blobContainer.Value;
            var blobRef = GetBlobRef(storageFileId, blob);

            if (!blobRef.Exists())
                return;

            blobRef.Delete();
        }

        private static string GetBlobName(StorageFileId storageFileId)
        {
            return $"{storageFileId.Group}_{storageFileId.Id}.{storageFileId.Extension}";
        }
    }
}
using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Pdf.Storage.Pdf.Config;

namespace Pdf.Storage.Pdf.PdfStores
{
    public class AzureStorage : IStorage
    {
        private readonly Lazy<BlobContainerClient> _blobContainer;

        public AzureStorage(IOptions<AzureStorageConfig> azureConfig)
        {
            _blobContainer = new Lazy<BlobContainerClient>(() =>
            {
                var blobContainer = new BlobContainerClient(azureConfig.Value.StorageConnectionString, azureConfig.Value.ContainerName);
                blobContainer.CreateIfNotExists();
                return blobContainer;
            });
        }

        public void AddOrReplace(StorageData storageData)
        {
            var blobRef = GetBlobRef(storageData.StorageFileId);
            blobRef.Upload(BinaryData.FromBytes(storageData.Data), overwrite: true);
        }

        private BlobClient GetBlobRef(StorageFileId storageFileId)
        {
            return _blobContainer.Value.GetBlobClient(GetBlobName(storageFileId));
        }

        public StorageData Get(StorageFileId storageFileId)
        {
            var blobRef = GetBlobRef(storageFileId);

            if (!blobRef.Exists())
                throw new InvalidOperationException($"Tried to open non existent blob '{GetBlobName(storageFileId)}'");

            var asDataArray = blobRef.DownloadContent().Value.Content.ToArray();

            return new StorageData(storageFileId, asDataArray);
        }

        public void Remove(StorageFileId storageFileId)
        {
            GetBlobRef(storageFileId).DeleteIfExists();
        }

        private static string GetBlobName(StorageFileId storageFileId)
        {
            return $"{storageFileId.Group}_{storageFileId.Id}.{storageFileId.Extension}";
        }
    }
}

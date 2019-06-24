using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Pdf
{
    public class GoogleCloudPdfStorage : IStorage
    {
        private readonly GoogleCloudConfig _settings;

        public GoogleCloudPdfStorage(IOptions<GoogleCloudConfig> settings)
        {
            _settings = settings.Value;

            var googleAuthConfig = File.ReadAllText(_settings.GoogleAuthFile);

            _storageClient = StorageClient.Create(credential: GoogleCredential.FromJson(googleAuthConfig).CreateScoped(new List<string>
            {
                StorageService.Scope.DevstorageFullControl
            }));
        }

        private readonly StorageClient _storageClient;

        public void AddOrReplace(StorageData storageData)
        {
            using (Stream stream = new MemoryStream(storageData.Data))
            {
                _storageClient.UploadObject(_settings.GoogleBucketName, GetObjectName(storageData.StorageFileId),
                    "application/pdf", stream, null, null);
            }
        }

        public StorageData Get(StorageFileId storageFileId)
        {
            var pdfBytes = new MemoryStream();
            _storageClient.DownloadObject(_settings.GoogleBucketName, GetObjectName(storageFileId), pdfBytes, null, null);
            return new StorageData(storageFileId, pdfBytes.ToArray());
        }

        public void Remove(StorageFileId storageFileId)
        {
            _storageClient.DeleteObject(_settings.GoogleBucketName, GetObjectName(storageFileId));
        }

        private static string GetObjectName(StorageFileId storageFileId)
        {
            return $"{storageFileId.Group}_{storageFileId.Id}.{storageFileId.Extension}";
        }
    }
}
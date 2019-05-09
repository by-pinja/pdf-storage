using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using Pdf.Storage.Hangfire;

namespace Pdf.Storage.Pdf
{
    public class GoogleCloudPdfStorage : IStorage
    {
        private readonly AppSettings _settings;

        public GoogleCloudPdfStorage(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;

            var googleAuthConfig = File.ReadAllText(_settings.GoogleAuthFile);

            _storageClient = StorageClient.Create(credential: GoogleCredential.FromJson(googleAuthConfig).CreateScoped(new List<string>
            {
                StorageService.Scope.DevstorageFullControl
            }));
        }

        private readonly StorageClient _storageClient;

        private string GetObjectName(string groupId, string fileId)
        {
            return $"{groupId}_{fileId}.pdf";
        }

        public void AddOrReplacePdf(StoredPdf pdf)
        {
            using (Stream stream = new MemoryStream(pdf.Data))
            {
                _storageClient.UploadObject(_settings.GoogleBucketName, GetObjectName(pdf.Group, pdf.Id),
                    "application/pdf", stream, null, null);
            }
        }

        public StoredPdf GetPdf(string groupId, string pdfId)
        {
            var pdfBytes = new MemoryStream();
            _storageClient.DownloadObject(_settings.GoogleBucketName, GetObjectName(groupId, pdfId), pdfBytes, null, null);
            return new StoredPdf(groupId, pdfId, pdfBytes.ToArray());
        }

        public void RemovePdf(string groupId, string pdfId)
        {
            _storageClient.DeleteObject(_settings.GoogleBucketName, GetObjectName(groupId, pdfId));
        }
    }
}
using System;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Pdf.Storage.Pdf
{
    public class AwsS3PdfStore : IPdfStorage
    {
        private readonly IAmazonS3 s3Client;

        private string bucketName;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;

        public AwsS3PdfStore(IOptions<AppSettings> options)
        {
            this.s3Client = new AmazonS3Client(bucketRegion);
            this.s3Client.EnsureBucketExistsAsync(bucketName);
            this.bucketName = options.Value.AwsS3BucketName ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AwsS3BucketName)}");
        }

        public void AddOrReplacePdf(StoredPdf pdf)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = GetKey(pdf.Group, pdf.Id),
                InputStream = new MemoryStream(pdf.Data)
            };

            this.s3Client.PutObjectAsync(putRequest).Wait();
        }

        public StoredPdf GetPdf(string groupId, string pdfId)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = GetKey(groupId, pdfId)
            };
            using (GetObjectResponse response = this.s3Client.GetObjectAsync(request).Result)
            using (Stream responseStream = response.ResponseStream)
            using (var memstream = new MemoryStream())
            {
                response.ResponseStream.CopyTo(memstream);
                return new StoredPdf(groupId, pdfId, memstream.ToArray());
            }
        }

        public void RemovePdf(string groupId, string pdfId)
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = GetKey(groupId, pdfId)
            };

            this.s3Client.DeleteObjectAsync(deleteObjectRequest).Wait();
        }

        private string GetKey(string groupId, string pdfId)
        {
            return $"{groupId}_{pdfId}";
        }
    }
}
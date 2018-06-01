using System;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Pdf.Storage.Config;

namespace Pdf.Storage.Pdf
{
    public class AwsS3PdfStore : IPdfStorage
    {
        private readonly IAmazonS3 s3Client;
        private string bucketName;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;

        public AwsS3PdfStore(IOptions<AwsS3Config> options)
        {
            var region = RegionEndpoint.EnumerableAllRegions
                .ToList()
                .SingleOrDefault(x => x.SystemName == options.Value.AwsRegion)
                ?? throw new InvalidOperationException($"Cannot resolve {nameof(options.Value.AwsRegion)} from valid options {String.Join(", ", RegionEndpoint.EnumerableAllRegions.Select(x => x.SystemName))}");

            var config = new AmazonS3Config
            {
                RegionEndpoint = region,
                ServiceURL = options.Value.AwsServiceURL ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AwsServiceURL)}"),
                ForcePathStyle = true
            };

            this.s3Client = new AmazonS3Client(
                options.Value.AccessKey ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AccessKey)}"),
                options.Value.SecretKey ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AccessKey)}"),
                config);

            this.bucketName = options.Value.AwsS3BucketName ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AwsS3BucketName)}");
            this.s3Client.EnsureBucketExistsAsync(bucketName);
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
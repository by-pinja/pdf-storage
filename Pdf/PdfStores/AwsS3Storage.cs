using System;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Pdf.Storage.Config;
using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Pdf
{
    public class AwsS3Storage : IStorage
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsS3Storage(IOptions<AwsS3Config> options)
        {
            var region = RegionEndpoint.EnumerableAllRegions
                .ToList()
                .SingleOrDefault(x => x.SystemName == options.Value.AwsRegion)
                ?? throw new InvalidOperationException($"Cannot resolve {nameof(options.Value.AwsRegion)} ({options.Value.AwsRegion}) from valid options {String.Join(", ", RegionEndpoint.EnumerableAllRegions.Select(x => x.SystemName))}");

            var config = new AmazonS3Config
            {
                RegionEndpoint = region,
                ForcePathStyle = true
            };

            if (options.Value.AwsServiceURL != null)
            {
                config.ServiceURL = options.Value.AwsServiceURL;
            }

            _s3Client = new AmazonS3Client(
                options.Value.AccessKey ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AccessKey)}"),
                options.Value.SecretKey ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AccessKey)}"),
                config);

            _bucketName = options.Value.AwsS3BucketName ?? throw new InvalidOperationException($"Missing configuration {nameof(options.Value.AwsS3BucketName)}");
            _s3Client.EnsureBucketExistsAsync(_bucketName);
        }

        public void AddOrReplace(StorageData storageData)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = GetKey(storageData.StorageFileId),
                InputStream = new MemoryStream(storageData.Data)
            };

            _s3Client.PutObjectAsync(putRequest).Wait();
        }

        public StorageData Get(StorageFileId storageFileId)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = GetKey(storageFileId)
            };

            using GetObjectResponse response = this._s3Client.GetObjectAsync(request).Result;
            using Stream responseStream = response.ResponseStream;
            using var memstream = new MemoryStream();

            response.ResponseStream.CopyTo(memstream);
            return new StorageData(storageFileId, memstream.ToArray());
        }

        public void Remove(StorageFileId storageFileId)
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = GetKey(storageFileId)
            };

            _s3Client.DeleteObjectAsync(deleteObjectRequest).Wait();
        }

        private string GetKey(StorageFileId storageFileId)
        {
            return $"{storageFileId.Group}_{storageFileId.Id}_{storageFileId.Extension}";
        }
    }
}

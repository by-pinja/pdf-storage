using Amazon.S3;

namespace Pdf.Storage.Pdf
{
    public class AwsS3PdfStore : IPdfStorage
    {
        private readonly IAmazonS3 s3Client;

        public AwsS3PdfStore(IAmazonS3 s3Client)
        {
            this.s3Client = s3Client;
            this.s3Client.EnsureBucketExistsAsync("pdf-storage");
        }

        public void AddOrReplacePdf(StoredPdf pdf)
        {
            this.s3Client.PutObjectAsync()
            throw new System.NotImplementedException();
        }

        public StoredPdf GetPdf(string groupId, string pdfId)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePdf(string groupId, string pdfId)
        {
            throw new System.NotImplementedException();
        }
    }
}
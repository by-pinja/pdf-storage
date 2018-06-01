namespace Pdf.Storage.Config
{
    public class AwsS3Config
    {
        public string AwsS3BucketName { get; set; } = "pdf-storage-master";
        public string AccessKey { get; set; } = "thisisaccesskey";
        public string SecretKey { get; set; } = "ThisIsSecretKey";
        public string AwsServiceURL { get; set; } = "http://localhost:900";
        public string AwsRegion { get; set; } = "EUCentral1";
    }
}

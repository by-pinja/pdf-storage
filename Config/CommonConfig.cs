namespace Pdf.Storage
{
    public class CommonConfig
    {
        public string BaseUrl { get; set; }

        // Configure this is chromium for puppeteer is preinstalled, like in container installment.
        public string PuppeteerChromiumPath { get; set; }
    }
}
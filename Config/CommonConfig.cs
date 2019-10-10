namespace Pdf.Storage
{
    public class CommonConfig
    {
        public string BaseUrl { get; set; }

        // Configure this is chromium for puppeteer is preinstalled, like in container installment.
        public string PuppeteerChromiumPath { get; set; }

        // If empty, only localhost is allowed.
        // To allow any ip addreess add "*".
        public string[] AllowedIpAddresses { get; set; } = new string[] {};

        // If username and password is set, then authentication for hangfire is enabled.
        public string HangfireDashboardUser { get; set; }
        public string HangfireDashboardPassword { get; set; }
    }
}
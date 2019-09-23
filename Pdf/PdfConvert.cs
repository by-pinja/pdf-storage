using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Pdf.Storage.Pdf
{
    public class PdfConvert : IPdfConvert
    {
        private readonly ILogger<PdfConvert> _logger;
        private readonly IOptions<CommonConfig> _settings;
        private readonly string _chromiumPath;

        public PdfConvert(ILogger<PdfConvert> logger, IOptions<CommonConfig> settings)
        {
            _logger = logger;
            _settings = settings;
            _chromiumPath = _settings.Value.PuppeteerChromiumPath ?? new BrowserFetcher().GetExecutablePath(BrowserFetcher.DefaultRevision);
        }

        public byte[] CreatePdfFromHtml(string html, JObject options)
        {
            var tempDir = ResolveTemporaryDirectory();

            try
            {
                File.WriteAllText(Path.Combine(tempDir, "source.html"), html);

                var data = GeneratePdf(tempDir).Result;

                return data;
            }
            finally
            {
                _logger.LogInformation($"Removing temporary folder: {tempDir}");
                //Directory.Delete(tempDir, true);
            }
        }

        private async Task<byte[]> GeneratePdf(string tempPath)
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = _chromiumPath,
                Headless = true,
                IgnoreHTTPSErrors = true,
                Args = new [] { "--no-sandbox", "--disable-dev-shm-usage", "--incognito", "--disable-gpu", "--disable-software-rasterizer" },
                EnqueueTransportMessages = false
            });

            var page = await browser.NewPageAsync();
            await page.GoToAsync($"file:///{Path.Combine(tempPath, "source.html")}");
            return await page.PdfDataAsync(new PdfOptions { Format = PaperFormat.A4 });
        }

        private string ResolveTemporaryDirectory()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}

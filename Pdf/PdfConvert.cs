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

        public PdfConvert(ILogger<PdfConvert> logger, IOptions<CommonConfig> settings)
        {
            _logger = logger;
            _settings = settings;
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
                ExecutablePath = "/usr/bin/chromium-browser",
                Headless = true,
                Timeout = 10*1000,
                IgnoreHTTPSErrors = true,
                DumpIO = true,
                Args = new [] { "--no-sandbox", "--disable-dev-shm-usage", "--incognito", "--disable-gpu", "--disable-software-rasterizer" },
                EnqueueTransportMessages = false
                // Args = new[]
                // {
                //     "--no-sandbox",
                //     "--disable-setuid-sandbox",
                //     "--incognito",
                //     "--disable-dev-shm-usage",
                //     "--disable-gpu",
                //     "--headless",
                //     "--disable-software-rasterizer"
                // }
            });

            var page = await browser.NewPageAsync();
            await page.GoToAsync($"file:///{Path.Combine(tempPath, "source.html")}");
            _logger.LogInformation("foobarbarbar");
            await page.PdfAsync(Path.Combine(tempPath, "output.pdf"), new PdfOptions { Format = PaperFormat.A4 });
            return File.ReadAllBytes(Path.Combine(tempPath, "output.pdf")).ToArray();
        }

        private string ResolveTemporaryDirectory()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}

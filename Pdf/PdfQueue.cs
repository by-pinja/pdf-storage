using System;
using System.Linq;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf.PdfStores;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using PuppeteerSharp;
using Microsoft.Extensions.Options;
using PuppeteerSharp.Media;

namespace Pdf.Storage.Pdf
{
    public class PdfQueue : IPdfQueue
    {
        private readonly PdfDataContext _context;
        private readonly IStorage _storage;
        private readonly IMqMessages _mqMessages;
        private readonly TemplatingEngine _templatingEngine;
        private readonly ILogger<PdfQueue> _logger;
        private readonly string _chromiumPath;

        public PdfQueue(
            PdfDataContext context,
            IStorage storage,
            IMqMessages mqMessages,
            TemplatingEngine templatingEngine,
            IOptions<CommonConfig> settings,
            ILogger<PdfQueue> logger)
        {
            _context = context;
            _storage = storage;
            _mqMessages = mqMessages;
            _templatingEngine = templatingEngine;
            _logger = logger;
            _chromiumPath = settings.Value.PuppeteerChromiumPath ?? new BrowserFetcher().GetExecutablePath(BrowserFetcher.DefaultRevision);
        }

        public void CreatePdf(Guid pdfEntityId)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);
            var rawData = _context.RawData.Single(x => x.ParentId == pdfEntityId);

            var templatedHtml = _templatingEngine.Render(rawData.Html, rawData.TemplateData);
            templatedHtml = TemplateUtils.AddWaitForAllPageElementsFixToHtml(templatedHtml);
            var data = GeneratePdfDataFromHtml(pdfEntityId, templatedHtml).GetAwaiter().GetResult();

            _storage.AddOrReplace(new StorageData(new StorageFileId(entity), data));
            _storage.AddOrReplace(new StorageData(new StorageFileId(entity, "html"), Encoding.UTF8.GetBytes(templatedHtml)));

            entity.Processed = true;

            _mqMessages.PdfGenerated(entity.GroupId, entity.FileId);

            _context.SaveChanges();
        }

        private async Task<byte[]> GeneratePdfDataFromHtml(Guid id, string html)
        {
            _logger.LogDebug($"Generating pdf from {id}");

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = _chromiumPath,
                Headless = true,
                IgnoreHTTPSErrors = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage", "--incognito", "--disable-gpu", "--disable-software-rasterizer" },
                EnqueueTransportMessages = false
            });

            var page = await browser.NewPageAsync();

            await page.SetContentAsync(html,
            new NavigationOptions
            {
                Timeout = 15 * 1000,
                WaitUntil = new[]
                {
                    WaitUntilNavigation.Load,
                    WaitUntilNavigation.DOMContentLoaded
                }
            });

            var result = await page.PdfDataAsync(new PdfOptions { Format = PaperFormat.A4 });
            await page.CloseAsync();
            await browser.CloseAsync();

            return result;
        }
    }
}

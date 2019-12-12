using System;
using System.Linq;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf.PdfStores;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using PuppeteerSharp;
using Microsoft.Extensions.Options;
using PuppeteerSharp.Media;
using System.Text;

namespace Pdf.Storage.Pdf
{
    public class PdfQueue : IPdfQueue
    {
        private readonly PdfDataContext _context;
        private readonly IStorage _storage;
        private readonly IMqMessages _mqMessages;
        private readonly ILogger<PdfQueue> _logger;
        private readonly string _chromiumPath;

        public PdfQueue(
            PdfDataContext context,
            IStorage storage,
            IMqMessages mqMessages,
            IOptions<CommonConfig> settings,
            ILogger<PdfQueue> logger)
        {
            _context = context;
            _storage = storage;
            _mqMessages = mqMessages;
            _logger = logger;
            _chromiumPath = settings.Value.PuppeteerChromiumPath ?? new BrowserFetcher().GetExecutablePath(BrowserFetcher.DefaultRevision);
        }

        public void CreatePdf(Guid pdfEntityId)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);
            var htmlFromStorage = _storage.Get(new StorageFileId(entity.GroupId, entity.Id.ToString(), "html"));
            var data = GeneratePdfDataFromHtml(pdfEntityId, Encoding.UTF8.GetString(htmlFromStorage.Data)).GetAwaiter().GetResult();

            _storage.AddOrReplace(new StorageData(new StorageFileId(entity), data));

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

            byte[] result;

            try
            {
                var page = await browser.NewPageAsync();

                await page.SetContentAsync(html,
                    new NavigationOptions
                    {
                        Timeout = 15 * 1000,
                        WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded }
                    });

                result = await page.PdfDataAsync(new PdfOptions { Format = PaperFormat.A4 });
                await page.CloseAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to generate pdf from {id}");
                throw;
            }
            finally
            {
                await browser.CloseAsync();
            }


            return result;
        }
    }
}

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
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public class PdfQueue : IPdfQueue
    {
        private readonly PdfDataContext _context;
        private readonly IStorage _storage;
        private readonly IMqMessages _mqMessages;
        private readonly ILogger<PdfQueue> _logger;
        private readonly string _chromiumPath;
        private readonly TemplateCacheService _templateCache;

        public PdfQueue(
            PdfDataContext context,
            IStorage storage,
            IMqMessages mqMessages,
            IOptions<CommonConfig> settings,
            ILogger<PdfQueue> logger,
            TemplateCacheService templateCache)
        {
            _context = context;
            _storage = storage;
            _mqMessages = mqMessages;
            _logger = logger;
            _chromiumPath = settings.Value.PuppeteerChromiumPath ?? new BrowserFetcher().GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);
            _templateCache = templateCache;
        }

        public void CreatePdf(Guid pdfEntityId)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);
            var template = _templateCache.Get(pdfEntityId) ??
                Encoding.UTF8.GetString(_storage.Get(new StorageFileId(entity, "html")).Data) ?? // in case template was already persisted
                throw new InvalidOperationException($"Html template missing for pdf entity id {pdfEntityId}");

            // Persist the template in storage
            _storage.AddOrReplace(new StorageData(new StorageFileId(entity, "html"), Encoding.UTF8.GetBytes(template)));
            
            var data = GeneratePdfDataFromHtml(pdfEntityId, template, entity.Options).GetAwaiter().GetResult();

            _storage.AddOrReplace(new StorageData(new StorageFileId(entity, "pdf"), data));

            entity.Processed = true;

            _mqMessages.PdfGenerated(entity.GroupId, entity.FileId);

            _context.SaveChanges();

            _templateCache.Remove(pdfEntityId);
        }

        private async Task<byte[]> GeneratePdfDataFromHtml(Guid id, string html, JObject options)
        {
            _logger.LogDebug($"Generating pdf from {id}");

            IBrowser browser = default;
            IPage page = default;

            try
            {
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    ExecutablePath = _chromiumPath,
                    Headless = true,
                    IgnoreHTTPSErrors = true,
                    Args = new[] { "--no-sandbox", "--disable-dev-shm-usage", "--incognito", "--disable-gpu", "--disable-software-rasterizer" },
                    EnqueueTransportMessages = false
                });

                page = await browser.NewPageAsync();

                await page.SetContentAsync(html,
                    new NavigationOptions
                    {
                        Timeout = 15 * 1000,
                        WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded }
                    });

                var defaultPdfOptions = new PdfOptions
                {
                    Format = options.ContainsKey("Width") && options.ContainsKey("Height") ?
                                    new PaperFormat(
                                    	options["Width"].Value<decimal>(),
                                    	options["Height"].Value<decimal>()
                                    ) :
                                    PaperFormat.A4
                };

                return await page.PdfDataAsync(new PdfOptions
                {
                    Format = defaultPdfOptions.Format,
                    DisplayHeaderFooter = options.ContainsKey("footerTemplate") || options.ContainsKey("headerTemplate"),
                    FooterTemplate = options.ContainsKey("footerTemplate") ? options["footerTemplate"].Value<string>() : defaultPdfOptions.FooterTemplate,
                    HeaderTemplate = options.ContainsKey("headerTemplate") ? options["headerTemplate"].Value<string>() : defaultPdfOptions.HeaderTemplate,
                    PrintBackground = options.ContainsKey("printBackground") ? options["printBackground"].Value<bool>() : defaultPdfOptions.PrintBackground,
                    PreferCSSPageSize = options.ContainsKey("preferCSSPageSize") ? options["preferCSSPageSize"].Value<bool>() : defaultPdfOptions.PreferCSSPageSize,
                    PageRanges = options.ContainsKey("pageRanges") ? options["pageRanges"].Value<string>() : defaultPdfOptions.PageRanges,
                    MarginOptions = new MarginOptions
                    {
                        Top = options.ContainsKey("marginTop") ? options["marginTop"].Value<string>() : defaultPdfOptions.MarginOptions.Top,
                        Bottom = options.ContainsKey("marginBottom") ? options["marginBottom"].Value<string>() : defaultPdfOptions.MarginOptions.Bottom,
                        Left = options.ContainsKey("marginLeft") ? options["marginLeft"].Value<string>() : defaultPdfOptions.MarginOptions.Left,
                        Right = options.ContainsKey("marginRight") ? options["marginRight"].Value<string>() : defaultPdfOptions.MarginOptions.Right,
                    }
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to generate pdf from {id}");
                throw;
            }
            finally
            {
                await page?.CloseAsync();
                page?.Dispose();
                await browser?.CloseAsync();
                browser?.Dispose();
            }
        }
    }
}

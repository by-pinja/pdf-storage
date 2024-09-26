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
        private readonly IOptions<CommonConfig> _settings;
        private readonly ILogger<PdfQueue> _logger;

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
            _settings = settings;
            _logger = logger;
        }

        public void CreatePdf(Guid pdfEntityId)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);
            var htmlFromStorage = _storage.Get(new StorageFileId(entity, "html"));
            var data = GeneratePdfDataFromHtml(pdfEntityId, Encoding.UTF8.GetString(htmlFromStorage.Data),
                entity.Options).GetAwaiter().GetResult();

            _storage.AddOrReplace(new StorageData(new StorageFileId(entity), data));

            entity.Processed = true;

            _mqMessages.PdfGenerated(entity.GroupId, entity.FileId);

            _context.SaveChanges();
        }

        private async Task<byte[]> GeneratePdfDataFromHtml(Guid id, string html, JObject options)
        {
            _logger.LogDebug($"Generating pdf from {id}");

            IBrowser browser = default;
            IPage page = default;

            try
            {
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    IgnoreHTTPSErrors = true,
                    Args = ["--no-sandbox", "--disable-dev-shm-usage", "--incognito", "--disable-gpu", "--disable-software-rasterizer"],
                    EnqueueTransportMessages = false
                };

                if(_settings.Value.PuppeteerChromiumPath != default)
                {
                    launchOptions.ExecutablePath = _settings.Value.PuppeteerChromiumPath;
                }

                browser = await Puppeteer.LaunchAsync(launchOptions);

                page = await browser.NewPageAsync();

                await page.SetContentAsync(html,
                    new NavigationOptions
                    {
                        Timeout = 15 * 1000,
                        WaitUntil = [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded]
                    });

                var width = options.GetValue("width", StringComparison.OrdinalIgnoreCase);
                var height = options.GetValue("height", StringComparison.OrdinalIgnoreCase);

                var defaultPdfOptions = new PdfOptions
                {
                    Format = width != null && height != null ?
                                    new PaperFormat(
                                        width.Value<decimal>(),
                                        height.Value<decimal>()
                                    ) :
                                    PaperFormat.A4
                };

                return await page.PdfDataAsync(new PdfOptions
                {
                    Format = options.ContainsKey("format") ? Format(options["format"].Value<string>()) : defaultPdfOptions.Format,
                    DisplayHeaderFooter = options.ContainsKey("footerTemplate") || options.ContainsKey("headerTemplate"),
                    FooterTemplate = options.ContainsKey("footerTemplate") ? options["footerTemplate"].Value<string>() : defaultPdfOptions.FooterTemplate,
                    HeaderTemplate = options.ContainsKey("headerTemplate") ? options["headerTemplate"].Value<string>() : defaultPdfOptions.HeaderTemplate,
                    PrintBackground = options.ContainsKey("printBackground") ? options["printBackground"].Value<bool>() : defaultPdfOptions.PrintBackground,
                    Landscape = options.ContainsKey("landscape") ? options["landscape"].Value<bool>() : defaultPdfOptions.Landscape,
                    PreferCSSPageSize = options.ContainsKey("preferCSSPageSize") ? options["preferCSSPageSize"].Value<bool>() : defaultPdfOptions.PreferCSSPageSize,
                    PageRanges = options.ContainsKey("pageRanges") ? options["pageRanges"].Value<string>() : defaultPdfOptions.PageRanges,
                    Scale = options.ContainsKey("scale") ? options["scale"].Value<decimal>() : defaultPdfOptions.Scale,
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

        private static PaperFormat Format(string format) => format switch {
            "Letter" => PaperFormat.Letter,
            "Legal" => PaperFormat.Legal,
            "Tabloid" => PaperFormat.Tabloid,
            "Ledger" => PaperFormat.Ledger,
            "A0" => PaperFormat.A0,
            "A1" => PaperFormat.A1,
            "A2" => PaperFormat.A2,
            "A3" => PaperFormat.A3,
            "A4" => PaperFormat.A4,
            "A5" => PaperFormat.A5,
            "A6" => PaperFormat.A6,
            _ => PaperFormat.A4,
        };
    }
}

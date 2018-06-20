using System.Linq;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;

namespace Pdf.Storage.Pdf
{
    public class CleanUpCronJob
    {
        private readonly PdfDataContext _context;
        private readonly ILogger<CleanUpCronJob> _logger;

        public CleanUpCronJob(PdfDataContext context, ILogger<CleanUpCronJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Execute()
        {
            var entitiesToDelete = _context.RawData
                .Join(_context.PdfFiles, x => x.ParentId, x => x.Id, (raw, file) => new { raw, processed = file.Processed })
                .Where(x => x.processed)
                .Take(500)
                .Select(x => x.raw);

            _context.RawData.RemoveRange(entitiesToDelete);

            _logger
                .LogInformation($"Cleared up raw data of pdfs: {string.Join(", ", entitiesToDelete.Take(5).Select(x => x.ParentId.ToString()))} ...");

            _context.SaveChanges();
        }
    }
}
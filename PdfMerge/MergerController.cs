using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pdf.Storage.Data;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Mq;

namespace Pdf.Storage.PdfMerge
{
    public class MergerController: Controller
    {
        private readonly IHangfireQueue _backgroundJob;
        private readonly PdfDataContext _context;
        private readonly IMqMessages _mqMessages;
        private readonly ILogger<MergerController> _logger;
        private readonly AppSettings _settings;

        public MergerController(
            IHangfireQueue backgroundJob,
            PdfDataContext context,
            IOptions<AppSettings> settings,
            IMqMessages mqMessages,
            ILogger<MergerController> logger)
        {
            _backgroundJob = backgroundJob;
            _context = context;
            _mqMessages = mqMessages;
            _logger = logger;
            _settings = settings.Value;
        }

        [HttpPost("v1/merge/{groupId}/")]
        public IActionResult MergePdfs(string groupId, [Required][FromBody] PdfMergeRequest request)
        {
            if (request.PdfIds.Length < 1)
                return BadRequest("Attleast one pdf must be defined, current length 0");

            var missingPdfFiles = MissingPdfFiles(request.PdfIds, groupId).ToList();

            if (!missingPdfFiles.Any())
            {
                var message = $"Pdf files not found, missing files from group '{groupId}' are '{missingPdfFiles.Aggregate("", (a, b) => $"{a}, {b}").Trim(',')}'";

                _logger.LogWarning($"Requested merge but it failed: {message}");

                return BadRequest(message);
            }

            var entity = _context.PdfFiles.Add(new PdfEntity(groupId)).Entity;

            var filePath = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{entity.FileId}.pdf";

            request.PdfIds.ToList().ForEach(id =>
            {
                _mqMessages.PdfOpened(groupId, id);
                _context.PdfFiles.Single(x => x.FileId == id).Usage.Add(new PdfOpenedEntity());
            });

            _context.SaveChanges();

            _backgroundJob.Enqueue<IPdfMerger>(merger => merger.MergePdf(entity.GroupId, entity.FileId, request.PdfIds));

            return Accepted(new MergeResponse(entity.FileId, filePath));
        }

        private IEnumerable<string> MissingPdfFiles(string[] pdfIds, string groupId)
        {
            var pdfIdsInDatabase = _context.PdfFiles.Where(x => x.GroupId == groupId).Select(x => x.FileId);
            return pdfIds.Where(pdfId => pdfIdsInDatabase.Any(id => id == pdfId));
        }
    }
}

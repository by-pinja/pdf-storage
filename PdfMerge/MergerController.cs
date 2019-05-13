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
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.PdfStores;

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
        public ActionResult<MergeResponse> MergePdfs(string groupId, [Required][FromBody] PdfMergeRequest request)
        {
            if (request.PdfIds.Length < 1)
                return BadRequest("Attleast one pdf must be defined, current length 0");

            var underlayingPdfFiles = _context.PdfFiles
                .Where(x => x.GroupId == groupId && !x.Removed)
                .Where(x => request.PdfIds.Any(id => x.FileId == id))
                .ToList();

            var missingPdfFiles = request.PdfIds.Where(x => ! underlayingPdfFiles.Any(file => x == file.FileId));

            if (missingPdfFiles.Any())
            {
                var message = $"Pdf files not found, missing files from group '{groupId}' are '{missingPdfFiles.Aggregate("", (a, b) => $"{a}, {b}").Trim(',')}'";

                _logger.LogWarning($"Requested merge but it failed: {message}");

                return BadRequest(message);
            }

            var mergeEntity = _context.PdfFiles.Add(new PdfEntity(groupId, PdfType.Merge)).Entity;

            var filePath = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{mergeEntity.FileId}.pdf";

            request.PdfIds.ToList().ForEach(id =>
            {
                _mqMessages.PdfOpened(groupId, id);
                underlayingPdfFiles.Single(x => x.FileId == id).Usage.Add(new PdfOpenedEntity());
            });

            var entitiesToPriritize =
                underlayingPdfFiles
                    .Where(x => !x.Processed)
                    .Where(x => x.IsValidForHighPriority())
                    .ToList();

            entitiesToPriritize.ForEach(pdfEntity =>
            {
                pdfEntity.MarkAsHighPriority(
                    _backgroundJob.EnqueueWithHighPriority<IPdfQueue>(que => que.CreatePdf(pdfEntity.Id), originalJobId: pdfEntity.HangfireJobId));
            });

            _context.SaveChanges();

            mergeEntity.HangfireJobId = _backgroundJob.EnqueueWithHighPriority<IPdfMerger>(merger => merger.MergePdf(new StorageFileId(mergeEntity, "pdf"), request.PdfIds));
            _context.SaveChanges();

            return Accepted(new MergeResponse(mergeEntity.FileId, filePath));
        }
    }
}

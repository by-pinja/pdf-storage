using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class MergerController : Controller
    {
        private readonly IHangfireQueue _backgroundJob;
        private readonly PdfDataContext _context;
        private readonly IMqMessages _mqMessages;
        private readonly ILogger<MergerController> _logger;
        private readonly CommonConfig _settings;

        public MergerController(
            IHangfireQueue backgroundJob,
            PdfDataContext context,
            IOptions<CommonConfig> settings,
            IMqMessages mqMessages,
            ILogger<MergerController> logger)
        {
            _backgroundJob = backgroundJob;
            _context = context;
            _mqMessages = mqMessages;
            _logger = logger;
            _settings = settings.Value;
        }

        /// <summary>
        /// Merge two or more pdf:s as single document.
        /// </summary>
        /// <remarks>
        /// Sometimes you want to merge generated PDF:s as single larger paged document.
        ///
        /// Note: All pdf:s must be from single group, merging between groups isn't supported.
        /// </remarks>
        [HttpPost("v1/merge/{groupId}/")]
        public ActionResult<MergeResponse> MergePdfs(string groupId, [Required][FromBody] PdfMergeRequest request)
        {
            if (request?.PdfIds is null or [])
                return BadRequest("Atleast one pdf must be defined, current length 0");

            // Also make sure that there are no null/empty Ids in the set.
            var validRequestedIds = request.PdfIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray();

            if (validRequestedIds.Length != request.PdfIds.Length)
            {
                var invalidIds = request.PdfIds.Except(validRequestedIds).ToList();
                return BadRequest($"Invalid PDF IDs provided (null or empty): '{string.Join(", ", invalidIds.Select(x => x ?? "null"))}'");
            }

            var underlyingPdfFiles = _context.PdfFiles
                .Where(x => x.GroupId == groupId && !x.Removed)
                .Where(x => validRequestedIds.Contains(x.FileId))
                .ToList();

            var pdfLookup = underlyingPdfFiles
                .Where(x => !string.IsNullOrWhiteSpace(x.FileId))
                .ToDictionary(x => x.FileId);

            var missingPdfFiles = validRequestedIds.Where(x => !pdfLookup.ContainsKey(x)).ToList();

            if (missingPdfFiles.Any())
            {
                var message = $"Pdf files not found, missing files from group '{groupId}' are '{string.Join(", ", missingPdfFiles)}'";

                _logger.LogWarning($"Requested merge but it failed: {message}");

                return BadRequest(message);
            }

            var mergeEntity = _context.PdfFiles.Add(new PdfEntity(groupId, PdfType.Merge)).Entity;

            var filePath = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{mergeEntity.FileId}.pdf";

            foreach (var id in validRequestedIds)
            {
                _mqMessages.PdfOpened(groupId, id);
                pdfLookup[id].Usage.Add(new PdfOpenedEntity());
            }

            var entitiesToPrioritize = underlyingPdfFiles
                .Where(x => !x.Processed && x.IsValidForHighPriority())
                .ToList();

            foreach (var pdfEntity in entitiesToPrioritize)
            {
                pdfEntity.MarkAsHighPriority(
                    _backgroundJob.EnqueueWithHighPriority<IPdfQueue>(que => que.CreatePdf(pdfEntity.Id), originalJobId: pdfEntity.HangfireJobId));
            }

            _context.SaveChanges();

            var storageFile = new StorageFileId(mergeEntity, "pdf");

            mergeEntity.HangfireJobId = _backgroundJob.EnqueueWithHighPriority<IPdfMerger>(merger => merger.MergePdf(storageFile, validRequestedIds));

            _context.SaveChanges();

            return Accepted(new MergeResponse(mergeEntity.FileId, filePath));
        }
    }
}

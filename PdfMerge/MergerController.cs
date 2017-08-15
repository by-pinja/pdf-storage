using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pdf.Storage.Data;

namespace Pdf.Storage.PdfMerge
{
    public class MergerController: Controller
    {
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly PdfDataContext _context;
        private readonly AppSettings _settings;

        public MergerController(IBackgroundJobClient backgroundJob, PdfDataContext context, IOptions<AppSettings> settings)
        {
            _backgroundJob = backgroundJob;
            _context = context;
            _settings = settings.Value;
        }

        [HttpPost("v1/merge/{groupId}/")]
        public IActionResult MergePdfs(string groupId, [Required][FromBody] PdfMergeRequest request)
        {
            if (request.PdfIds.Length < 1)
                return BadRequest();

            if (!AllRequestedFilesExists(request.PdfIds, groupId))
                return BadRequest();

            var entity = _context.PdfFiles.Add(new PdfEntity(groupId)).Entity;

            var filePath = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{entity.FileId}.pdf";

            request.PdfIds.ToList().ForEach(id => _context.PdfFiles.Single(x => x.FileId == id).Usage.Add(new PdfOpenedEntity()));

            _context.SaveChanges();

            _backgroundJob.Enqueue<IPdfMerger>(merger => merger.MergePdf(entity.GroupId, entity.FileId, request.PdfIds));

            return Accepted(new MergeResponse(entity.FileId, filePath));
        }

        private bool AllRequestedFilesExists(string[] pdfIds, string groupId)
        {
            var pdfIdsInDatabase = _context.PdfFiles.Where(x => x.GroupId == groupId).Select(x => x.FileId);
            return pdfIds.All(pdfId => pdfIdsInDatabase.Any(id => id == pdfId));
        }
    }
}

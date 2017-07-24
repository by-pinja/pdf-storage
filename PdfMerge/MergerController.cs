using System;
using System.ComponentModel.DataAnnotations;
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
        private AppSettings _settings;

        public MergerController(IBackgroundJobClient backgroundJob, PdfDataContext context, IOptions<AppSettings> settings)
        {
            _backgroundJob = backgroundJob;
            _context = context;
            _settings = settings.Value;
        }

        [HttpPost("v1/merge/{groupId}/")]
        public IActionResult MergePdfs(string groupId, [Required] MergeRequest[] requests)
        {
            var entity = _context.PdfFiles.Add(new PdfEntity(groupId)).Entity;

            var filePath = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{entity.FileId}.pdf";

            _backgroundJob.Enqueue<IPdfMerger>(merger => merger.MergePdf(entity.FileId, requests));

            return Accepted(new MergeResponse(entity.FileId, filePath));
        }
    }
}

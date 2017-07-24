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
        public IActionResult MergePdfs(string groupId, [Required][FromBody] MergeRequest[] requests)
        {
            if (requests.Length < 2)
                return BadRequest();

            if (!AllRequestedFilesExists(requests))
                return BadRequest();

            var entity = _context.PdfFiles.Add(new PdfEntity(groupId)).Entity;

            var filePath = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{entity.FileId}.pdf";

            _context.SaveChanges();

            _backgroundJob.Enqueue<IPdfMerger>(merger => merger.MergePdf(entity.GroupId, entity.FileId, requests));

            return Accepted(new MergeResponse(entity.FileId, filePath));
        }

        private bool AllRequestedFilesExists(MergeRequest[] requests)
        {
            var groups = requests.Select(x => x.Group).Distinct();
            var keys = _context.PdfFiles.Where(x => groups.Any(g => g == x.GroupId)).Select(x => $"{x.GroupId}_{x.FileId}");
            return requests.All(r => keys.Any(k => k == $"{r.Group}_{r.PdfId}"));
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pdf.Storage.Data;
using Pdf.Storage.Test;

namespace Pdf.Storage.Pdf
{
    public class PdfController : Controller
    {
        private readonly PdfDataContext _context;
        private readonly IPdfStorage _pdfStorage;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly AppSettings _settings;

        public PdfController(PdfDataContext context, IPdfStorage pdfStorage, IOptions<AppSettings> settings, IBackgroundJobClient backgroundJob)
        {
            _context = context;
            _pdfStorage = pdfStorage;
            _backgroundJobs = backgroundJob;
            _settings = settings.Value;
        }

        [HttpPost("/v1/pdf/{groupId}/")]
        public IActionResult AddNewPdf([Required] string groupId, [FromBody] NewPdfRequest request)
        {
            var responses = request.RowData.ToList().Select(row =>
            {
                var entity = _context.PdfFiles.Add(new PdfEntity(groupId)).Entity;
                _context.SaveChanges();

                var templateData = TemplateDataUtils.GetTemplateData(request.BaseData, row);

                _backgroundJobs.Enqueue<IPdfQueue>(que => que.CreatePdf(entity.Id, request.Html, templateData));

                var pdfUri = $"{_settings.BaseUrl}/v1/pdf/{groupId}/{entity.FileId}.pdf";

                return new NewPdfResponse(entity.FileId, entity.GroupId, pdfUri, row);
            });

            return StatusCode(202, responses.ToList());
        }

        [HttpGet("/v1/pdf/{groupId}/{pdfId}.pdf")]
        public IActionResult GetPdf(string groupId, string pdfId)
        {
            var pdfEntity = _context.PdfFiles.SingleOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

            if (pdfEntity == null)
            {
                return new ContentResult
                {
                    Content = "404: PDF doesn't exists. Check url and if it is correct contact customer support.",
                    ContentType = "text/html",
                    StatusCode = 404
                };
            }

            if (!pdfEntity.Processed)
            {
                return new ContentResult
                {
                    Content = "404: PDF is waiting to be processed. Please try again later. This should not take longer than few minutes.",
                    ContentType = "text/html",
                    StatusCode = 404
                };
            }

            var pdf = _pdfStorage.GetPdf(pdfEntity.GroupId, pdfEntity.FileId);

            return new FileStreamResult(new MemoryStream(pdf.Data), "application/pdf");
        }
    }
}

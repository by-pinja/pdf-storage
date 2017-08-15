using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pdf.Storage.Data;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.Test;

namespace Pdf.Storage.Pdf
{
    public class PdfController : Controller
    {
        private readonly PdfDataContext _context;
        private readonly IPdfStorage _pdfStorage;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IErrorPages _errorPages;
        private readonly AppSettings _settings;

        public PdfController(PdfDataContext context, IPdfStorage pdfStorage, IOptions<AppSettings> settings, IBackgroundJobClient backgroundJob, IErrorPages errorPages)
        {
            _context = context;
            _pdfStorage = pdfStorage;
            _backgroundJobs = backgroundJob;
            _errorPages = errorPages;
            _settings = settings.Value;
        }

        [Authorize(ActiveAuthenticationSchemes = "ApiKey")]
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
        public IActionResult GetPdf(string groupId, string pdfId, [FromQuery] bool noCount)
        {
            var pdfEntity = _context.PdfFiles.SingleOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

            if (pdfEntity == null)
            {
                return _errorPages.PdfNotFoundResponse();
            }

            if (!pdfEntity.Processed)
            {
                return _errorPages.PdfIsStillProcessingResponse();
            }

            var pdf = _pdfStorage.GetPdf(pdfEntity.GroupId, pdfEntity.FileId);

            if (!noCount)
            {
                pdfEntity.Usage.Add(new PdfOpenedEntity());
                _context.SaveChanges();
            }

            return new FileStreamResult(new MemoryStream(pdf.Data), "application/pdf");
        }
    }
}

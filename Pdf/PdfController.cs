using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Pdf.Storage.Data;
using Pdf.Storage.Test;

namespace Pdf.Storage.Pdf
{
    public class PdfController : Controller
    {
        private readonly IPdfConvert _pdfService;
        private readonly PdfDataContext _context;
        private readonly IPdfStorage _pdfStorage;

        public PdfController(IPdfConvert pdfService, PdfDataContext context, IPdfStorage pdfStorage)
        {
            _pdfService = pdfService;
            _context = context;
            _pdfStorage = pdfStorage;
        }

        [HttpPost("/v1/pdf/{groupId}/")]
        public IActionResult AddNewPdf([Required] string groupId, [FromBody] NewPdfRequest request)
        {
            var pdf = _pdfService.CreatePdfFromHtml(request.Html);
            var entity = _context.PdfFiles.Add(new PdfEntity(groupId, request.Html)).Entity;
            entity.Processed = true;
            _context.SaveChanges();

            _pdfStorage.AddPdf(new StoredPdf(entity.GroupId, entity.FileId, pdf.data));

            return StatusCode(202, new NewPdfResponse(entity.FileId, entity.FileId));
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

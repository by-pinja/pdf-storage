using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
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

            _pdfStorage.AddPdf(new StoredPdf(entity.GroupId, entity.FileId, pdf.data));

            return StatusCode(202, new NewPdfResponse(entity.FileId, entity.FileId));
        }

        [HttpGet("/v1/pdf/{groupId}/{pdfId}.pdf")]
        public IActionResult GetPdf(string groupId, string pdfId)
        {
            var pdf = _pdfStorage.GetPdf(groupId, pdfId);

            var stream = new MemoryStream(pdf.Data);
            return new FileStreamResult(stream, "application/pdf");
        }
    }
}

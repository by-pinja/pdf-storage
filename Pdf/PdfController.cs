using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Pdf.Service.Pdf
{
    public class PdfController: Controller
    {
        public PdfController(IPdfConvert pdfService)
        {
        }

        [HttpPost("/v1/pdf/{groupId}/")]
        public IActionResult AddNewPdf([Required] Guid groupId, [FromBody] NewPdfRequest request)
        {
            var id = "testi";
            return StatusCode(202, new NewPdfResponse(id, ""));
        }
    }
}

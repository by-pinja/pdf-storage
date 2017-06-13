using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pdf.Storage.Data;
using Pdf.Storage.Pdf.Dto;

namespace Pdf.Storage.Pdf
{
    public class PdfUsageCountController: Controller
    {
        private readonly PdfDataContext _context;

        public PdfUsageCountController(PdfDataContext context)
        {
            _context = context;
        }

        [HttpGet("/v1/usage/{groupId}")]
        public IActionResult GetSimpleCount([Required] string groupId)
        {
            var result = _context.PdfFiles
                .Include(x => x.Usage)
                .Where(x => x.GroupId == groupId && x.Processed)
                .Where(x => x.Usage.Any())
                .Select(x => new PdfUsageCountSimpleResponse(x.FileId, true))
                .ToList();

            return Ok(result);
        }

        [HttpGet("/v1/usage/{groupId}/{pdfId}.pdf")]
        public IActionResult GetSimpleCount([Required] string groupId, [Required] string pdfId)
        {
            var result = _context.PdfFiles
                .Include(x => x.Usage)
                .Single(x => x.GroupId == groupId && x.FileId == pdfId);

            return Ok(new PdfUsageCountResponse
            {
                Opened = result.Usage.Select(x => x.Stamp).ToList(),
            });
        }
    }
}

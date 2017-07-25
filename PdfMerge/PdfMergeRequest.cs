using System.ComponentModel.DataAnnotations;

namespace Pdf.Storage.PdfMerge
{
    public class PdfMergeRequest
    {
        public PdfMergeRequest() {}

        public PdfMergeRequest(params string[] ids)
        {
            PdfIds = ids;
        }

        [Required]
        public string[] PdfIds { get; set; }
    }
}

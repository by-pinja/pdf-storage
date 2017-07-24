using System.ComponentModel.DataAnnotations;

namespace Pdf.Storage.PdfMerge
{
    public class MergeRequest
    {
        [Required]
        public string Group { get; set; }

        [Required]
        public string PdfId { get; set; }
    }
}

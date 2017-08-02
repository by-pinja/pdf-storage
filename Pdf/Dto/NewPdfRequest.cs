using System.ComponentModel.DataAnnotations;

namespace Pdf.Storage.Pdf.Dto
{
    public class NewPdfRequest
    {
        [Required]
        public string Html { get; set; }

        [Required]
        public object BaseData { get; set; }

        [Required]
        public object[] RowData { get; set; } = new object[0];

        public string PdfOpenedCallback { get; set; }
    }
}
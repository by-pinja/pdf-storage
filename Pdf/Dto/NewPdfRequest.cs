using System.ComponentModel.DataAnnotations;

namespace Pdf.Storage.Pdf.Dto
{
    public class NewPdfRequest
    {
        [Required]
        public string Html { get; set; }
        public object BaseData { get; set; } = new {};
        [Required]
        public object[] RowData { get; set; } = new object[0];
    }
}
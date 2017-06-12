using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
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
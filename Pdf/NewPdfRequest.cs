using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public class NewPdfRequest
    {
        [Required]
        public string Html { get; set; }
        public JObject BaseData { get; set; } = new JObject();
        [Required]
        public JObject[] RowData { get; set; } = new JObject[0];
    }
}
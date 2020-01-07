using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf.Dto
{
    public class NewPdfRequest
    {
        [Required]
        public string Html { get; set; }

        [Required]
        public JObject BaseData { get; set; } = new JObject();

        [Required]
        public JObject[] RowData { get; set; } = new JObject[0];
        public JObject Options { get; set; } = new JObject();
    }
}
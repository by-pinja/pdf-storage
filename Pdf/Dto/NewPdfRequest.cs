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
        public JToken RowData { get; set; } = new JArray();

        public JObject Options { get; set; } = new JObject();
    }
}
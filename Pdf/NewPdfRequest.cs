using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Pdf.Service.Pdf
{
    public class NewPdfRequest
    {
        [Required]
        public string Html { get; set; }

        public JObject Data { get; set; } = new JObject();
    }
}
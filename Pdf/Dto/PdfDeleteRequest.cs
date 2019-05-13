using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pdf.Storage.Pdf.Dto
{
    public class PdfDeleteRequest
    {
        [Required]
        public string GroupId { get; set;}

        [Required]
        public string PdfId { get; set;}
    }
}
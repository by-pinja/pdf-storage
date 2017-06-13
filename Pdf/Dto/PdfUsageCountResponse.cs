using System;
using System.Collections.Generic;

namespace Pdf.Storage.Pdf.Dto
{
    public class PdfUsageCountResponse
    {
        public IEnumerable<DateTime> Opened { get; set; }
    }
}
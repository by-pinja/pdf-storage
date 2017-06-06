using System;

namespace Pdf.Service.Data
{
    public class PdfEntity
    {
        public Guid Id { get; protected set; }
        public Guid GroupId { get; protected set; }
        public string FileId { get; set; }
        public string OriginalHtml { get; set; }
        public bool Processed { get; set; }
        public int OpenedTimes { get; set; }
    }
}

using System;

namespace Pdf.Storage.Data
{
    public class PdfEntity
    {
        protected PdfEntity() {}

        public PdfEntity(string groupId, string originalHtml)
        {
            GroupId = groupId;
            OriginalHtml = originalHtml;
            FileId = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty);
        }

        public Guid Id { get; protected set; }
        public string GroupId { get; protected set; }
        public string FileId { get; protected set; }
        public string OriginalHtml { get; set; }
        public bool Processed { get; set; }
        public int OpenedTimes { get; set; }
    }
}

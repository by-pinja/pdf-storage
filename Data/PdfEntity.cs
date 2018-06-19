using System;
using System.Collections.Generic;

namespace Pdf.Storage.Data
{
    public class PdfEntity
    {
        protected PdfEntity() {}

        public PdfEntity(string groupId, PdfType type)
        {
            GroupId = groupId;
            FileId = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty);
            Created = DateTime.UtcNow;
            Type = type;
        }

        public Guid Id { get; protected set; }
        public string GroupId { get; protected set; }
        public string FileId { get; protected set; }
        public DateTime Created { get; protected set; }
        public bool Processed { get; set; }
        public bool Removed { get; set; }
        public PdfType Type  { get; set; }
        public int OpenedTimes { get; set; }
        public string HangfireJobId { get; set; }
        public ICollection<PdfOpenedEntity> Usage { get; protected set; } = new List<PdfOpenedEntity>();
        public bool IsValidForHighPriority() => !Processed && Type != PdfType.Merge && Type != PdfType.HighPriorityPdf && HangfireJobId != null;
    }
}

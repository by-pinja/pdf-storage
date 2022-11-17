using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Data
{
    public class PdfEntity
    {
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
        public PdfType Type  { get; protected set; }
        public int OpenedTimes { get; set; }
        public string HangfireJobId { get; set; } = string.Empty;
        public ICollection<PdfOpenedEntity> Usage { get; protected set; } = new List<PdfOpenedEntity>();
        public JObject Options { get; set; } = JObject.Parse("{}");

        public bool IsValidForHighPriority() => !Processed && Type == PdfType.Pdf && HangfireJobId != null;
        public void MarkAsHighPriority(string newHangfireJobId)
        {
            Type = PdfType.HighPriorityPdf;
            HangfireJobId = newHangfireJobId;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Pdf.Storage.Data
{
    public class PdfEntity
    {
        protected PdfEntity() {}

        public PdfEntity(string groupId)
        {
            GroupId = groupId;
            FileId = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty);
        }

        public Guid Id { get; protected set; }
        public string GroupId { get; protected set; }
        public string FileId { get; protected set; }
        public bool Processed { get; set; }
        public int OpenedTimes { get; set; }

        // TODO: Remove this after message queues are implemented.
        public string PdfOpenedCallbackUri { get; set; }

        public ICollection<PdfOpenedEntity> Usage { get; protected set; } = new List<PdfOpenedEntity>();
    }
}

using System;

namespace Pdf.Storage.Data
{
    public class PdfOpenedEntity
    {
        public PdfOpenedEntity()
        {
            Stamp = DateTime.UtcNow;
        }
        public Guid Id { get; protected set; }

        public DateTime Stamp { get; protected set; }
        public PdfEntity Parent { get; protected set; }
    }
}

using System;

namespace Pdf.Storage.Pdf
{
    public interface IPdfQueue
    {
        void CreatePdf(Guid pdfEntityId, string html, object templateData, object options);
    }
}
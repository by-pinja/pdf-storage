using System;
using System.Linq;
using Pdf.Storage.Data;
using Pdf.Storage.Test;

namespace Pdf.Storage.Pdf
{
    public class PdfQueue : IPdfQueue
    {
        private readonly PdfDataContext _context;
        private readonly IPdfStorage _pdfStorage;
        private readonly IPdfConvert _pdfConverter;

        public PdfQueue(PdfDataContext context, IPdfStorage pdfStorage, IPdfConvert pdfConverter)
        {
            _context = context;
            _pdfStorage = pdfStorage;
            _pdfConverter = pdfConverter;
        }

        public void CreatePdf(Guid pdfEntityId, string html, object templateData)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);

            var pdf = _pdfConverter.CreatePdfFromHtml(html, templateData);

            _pdfStorage.AddPdf(new StoredPdf(entity.GroupId, entity.FileId, pdf.data));

            entity.Processed = true;

            _context.SaveChanges();
        }
    }
}

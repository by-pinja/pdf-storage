using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Test;

namespace Pdf.Storage.Pdf
{
    public class PdfQueue : IPdfQueue
    {
        private readonly PdfDataContext _context;
        private readonly IPdfStorage _pdfStorage;
        private readonly IPdfConvert _pdfConverter;
        private readonly IMqMessages _mqMessages;

        public PdfQueue(PdfDataContext context, IPdfStorage pdfStorage, IPdfConvert pdfConverter, IMqMessages mqMessages)
        {
            _context = context;
            _pdfStorage = pdfStorage;
            _pdfConverter = pdfConverter;
            _mqMessages = mqMessages;
        }

        public void CreatePdf(Guid pdfEntityId, string html, object templateData)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);

            var pdf = _pdfConverter.CreatePdfFromHtml(html, templateData);

            _pdfStorage.AddOrReplacePdf(new StoredPdf(entity.GroupId, entity.FileId, pdf.data));

            entity.Processed = true;

            _context.SaveChanges();

            _mqMessages.PdfGenerated(entity.GroupId, entity.FileId);
        }
    }
}

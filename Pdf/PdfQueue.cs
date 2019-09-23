using System;
using System.Linq;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf.PdfStores;
using System.Text;

namespace Pdf.Storage.Pdf
{
    public class PdfQueue : IPdfQueue
    {
        private readonly PdfDataContext _context;
        private readonly IStorage _storage;
        private readonly IPdfConvert _pdfConverter;
        private readonly IMqMessages _mqMessages;
        private readonly TemplatingEngine _templatingEngine;

        public PdfQueue(PdfDataContext context, IStorage storage, IPdfConvert pdfConverter, IMqMessages mqMessages, TemplatingEngine templatingEngine)
        {
            _context = context;
            _storage = storage;
            _pdfConverter = pdfConverter;
            _mqMessages = mqMessages;
            _templatingEngine = templatingEngine;
        }

        public void CreatePdf(Guid pdfEntityId)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);
            var rawData = _context.RawData.Single(x => x.ParentId == pdfEntityId);

            var templatedHtml = _templatingEngine.Render(rawData.Html, rawData.TemplateData);
            //templatedHtml = TemplateUtils.AddWaitForAllPageElementsFixToHtml(templatedHtml);

            var data = _pdfConverter.CreatePdfFromHtml(templatedHtml, rawData.Options);

            _storage.AddOrReplace(new StorageData(new StorageFileId(entity), data));
            _storage.AddOrReplace(new StorageData(new StorageFileId(entity, "html"), Encoding.UTF8.GetBytes(templatedHtml)));

            entity.Processed = true;

            _mqMessages.PdfGenerated(entity.GroupId, entity.FileId);

            _context.SaveChanges();
        }
    }
}

using System;
using System.Linq;
using Hangfire;
using Microsoft.Extensions.Options;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Hangfire;
using Newtonsoft.Json.Linq;
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

        public PdfQueue(PdfDataContext context, IStorage storage, IPdfConvert pdfConverter, IMqMessages mqMessages)
        {
            _context = context;
            _storage = storage;
            _pdfConverter = pdfConverter;
            _mqMessages = mqMessages;
        }

        public void CreatePdf(Guid pdfEntityId)
        {
            var entity = _context.PdfFiles.Single(x => x.Id == pdfEntityId);
            var rawData = _context.RawData.Single(x => x.ParentId == pdfEntityId);

            var (data, html) = _pdfConverter.CreatePdfFromHtml(rawData.Html, JObject.Parse(rawData.TemplateData), JObject.Parse(rawData.Options));

            _storage.AddOrReplace(new StorageData(new StorageFileId(entity), data));
            _storage.AddOrReplace(new StorageData(new StorageFileId(entity, "html"), Encoding.Unicode.GetBytes(html)));

            entity.Processed = true;

            _mqMessages.PdfGenerated(entity.GroupId, entity.FileId);

            _context.SaveChanges();
        }
    }
}

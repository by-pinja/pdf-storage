using System.Collections.Generic;
using Pdf.Storage.Pdf;

namespace Pdf.Storage.Hangfire
{
    public class InMemoryPdfStorage : IStorage
    {
        private readonly Dictionary<string, StoredPdf> _localStore = new Dictionary<string, StoredPdf>();

        public void AddOrReplacePdf(StoredPdf pdf)
        {
            if (_localStore.ContainsKey(GetKey(pdf.Group, pdf.Id)))
                _localStore.Remove(GetKey(pdf.Group, pdf.Id));

            _localStore.Add(GetKey(pdf.Group, pdf.Id), pdf);
        }

        public StoredPdf GetPdf(string groupId, string pdfId)
        {
            return _localStore[GetKey(groupId, pdfId)];
        }

        public void RemovePdf(string groupId, string pdfId)
        {
            if (_localStore.ContainsKey(GetKey(groupId, pdfId)))
                _localStore.Remove(GetKey(groupId, pdfId));
        }

        private static string GetKey(string groupId, string pdfId)
        {
            return $"{groupId}_{pdfId}";
        }
    }
}

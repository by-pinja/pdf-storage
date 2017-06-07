using System.Collections.Generic;
using Pdf.Storage.Pdf;

namespace Pdf.Storage.Test
{
    public class InMemoryPdfStorage : IPdfStorage
    {
        private readonly Dictionary<string, StoredPdf> _localStore = new Dictionary<string, StoredPdf>();

        public void AddPdf(StoredPdf pdf)
        {
            _localStore.Add($"{pdf.Group}_{pdf.Id}", pdf);
        }

        public StoredPdf GetPdf(string groupId, string pdfId)
        {
            return _localStore[$"{groupId}_{pdfId}"];
        }
    }
}

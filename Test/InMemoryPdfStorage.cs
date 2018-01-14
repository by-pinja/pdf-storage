using System.Collections.Generic;
using Pdf.Storage.Pdf;

namespace Pdf.Storage.Hangfire
{
    public class InMemoryPdfStorage : IPdfStorage
    {
        // Currently theres issues with jobmanagement di, forces for static workaround: https://github.com/HangfireIO/Hangfire/issues/808
        private static readonly Dictionary<string, StoredPdf> LocalStore = new Dictionary<string, StoredPdf>();

        public void AddOrReplacePdf(StoredPdf pdf)
        {
            if (LocalStore.ContainsKey($"{pdf.Group}_{pdf.Id}"))
                LocalStore.Remove("{pdf.Group}_{pdf.Id}");

            LocalStore.Add($"{pdf.Group}_{pdf.Id}", pdf);
        }

        public StoredPdf GetPdf(string groupId, string pdfId)
        {
            return LocalStore[$"{groupId}_{pdfId}"];
        }
    }
}

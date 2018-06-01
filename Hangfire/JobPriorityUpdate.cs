using System.Collections.Generic;

namespace Pdf.Storage.Hangfire
{
    public class JobPriority
    {
        public void UpgradePdfPriority(string pdfGroup, string pdfId)
        {
        }

        private static IEnumerable<string> GetValidQueues => new string[] { "critical", "default"};
    }
}
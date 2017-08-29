using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Pdf.Storage.Util
{
    public class Uris
    {
        private readonly IOptions<AppSettings> _settings;

        public Uris(IOptions<AppSettings> settings)
        {
            _settings = settings;
        }

        public string PdfUri(string groupId, string pdfFileId)
        {
            return $"{_settings.Value.BaseUrl}/v1/pdf/{groupId}/{pdfFileId}.pdf";
        }
    }
}

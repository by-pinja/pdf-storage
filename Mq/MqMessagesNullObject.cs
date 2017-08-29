using Microsoft.Extensions.Logging;
using Pdf.Storage.Pdf;
using Pdf.Storage.Util;

namespace Pdf.Storage.Mq
{
    public class MqMessagesNullObject: IMqMessages
    {
        private readonly ILogger<MqMessagesNullObject> _logger;

        public MqMessagesNullObject(ILogger<MqMessagesNullObject> logger)
        {
            _logger = logger;
        }

        public void PdfGenerated(string groupId, string pdfId)
        {
            _logger.LogInformation($"{nameof(PdfGenerated)}: Mq disabled");
        }

        public void PdfOpened(string groupId, string pdfId)
        {
            _logger.LogInformation($"{nameof(PdfOpened)}: Mq disabled");
        }
    }
}

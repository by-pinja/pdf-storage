using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Util;
using RabbitMQ.Client;

namespace Pdf.Storage.Mq
{
    public class MqMessages : IMqMessages
    {
        private readonly ILogger<MqMessages> _logger;
        private readonly Uris _uris;
        private readonly MqConfig _mqConfig;
        private const string Exhcange = "Eventale";
        private const string PdfOpenedKey = "pdf-storage.v1.opened";
        private const string PdfGeneratedKey = "pdf-storage.v1.generated";

        public MqMessages(IOptions<MqConfig> mqConfig, ILogger<MqMessages> logger, Uris uris)
        {
            _logger = logger;
            _uris = uris;
            _mqConfig = mqConfig.Value;
        }

        public void PdfOpened(string groupId, string pdfId)
        {
            PublishMessage(groupId, pdfId, PdfOpenedKey);

        }

        public void PdfGenerated(string groupId, string pdfId)
        {
            PublishMessage(groupId, pdfId,  PdfGeneratedKey);
        }

        private void PublishMessage(string groupId, string pdfId, string routingKey)
        {
            var pdfUri = _uris.PdfUri(groupId, pdfId);

            using (var connection = new ConnectionFactory {HostName = _mqConfig.Host}.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var data = JObject.FromObject(new
                {
                    id = Guid.NewGuid(),
                    timeStamp = DateTime.UtcNow,
                    data = new
                    {
                        groupId = groupId,
                        pdfId = pdfId,
                        pdfUri = pdfUri,
                        timeStamp = DateTime.UtcNow,
                    }
                }).ToString();

                channel.BasicPublish(exchange: Exhcange,
                    routingKey: routingKey,
                    body: Encoding.UTF8.GetBytes(data));

                _logger.LogInformation($"Message '{routingKey}': {data}");
            }
        }
    }
}

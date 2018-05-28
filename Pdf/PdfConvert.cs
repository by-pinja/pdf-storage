using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Logging;

namespace Pdf.Storage.Pdf
{
    public class PdfConvert : IPdfConvert
    {
        private readonly INodeServices _nodeServices;
        private readonly ILogger<PdfConvert> _logger;

        public PdfConvert(INodeServices nodeServices, ILogger<PdfConvert> logger)
        {
            _nodeServices = nodeServices;
            _logger = logger;
        }

        public (byte[] data, string html) CreatePdfFromHtml(string html, object templateData, object options)
        {
            var pdf = _nodeServices.InvokeAsync<ExpandoObject>(@"./node/convert.js", html, templateData, options ?? new object()).Result;

            var data = pdf.SingleOrDefault(x => x.Key == "data").Value;

            if (data == null)
            {
                throw new InvalidOperationException("Didn't get data from node service.");
            }

            if (data is List<object> objects)
            {
                var pdfAsBytes = objects.Select(Convert.ToByte).ToArray();
                return (pdfAsBytes, html);
            }

            _logger.LogError($"Something unexpected occurred, cannot parse result from '{data}'");

            throw new InvalidOperationException($"Something unexpected occurred, cannot parse result from '{data}'");
        }
    }
}

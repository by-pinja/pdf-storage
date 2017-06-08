using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.NodeServices;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public class PdfConvert : IPdfConvert
    {
        private readonly INodeServices _nodeServices;
        public PdfConvert(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
        }

        public (byte[] data, string html) CreatePdfFromHtml(string html)
        {
            Console.WriteLine("Creting from: " + html);

            var pdf = _nodeServices.InvokeAsync<ExpandoObject>(@"./node/convert.js", html, new {}).Result;

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

            throw new InvalidOperationException($"Something unexpected occurred, cannot parse result from '{data}'");
        }
    }
}

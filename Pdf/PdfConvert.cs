using Microsoft.AspNetCore.NodeServices;

namespace Pdf.Service.Pdf
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
            dynamic pdf = _nodeServices.InvokeAsync<object>(@"./node/convert.js", html).Result;
            return (new byte[]{}, html);
        }
    }
}

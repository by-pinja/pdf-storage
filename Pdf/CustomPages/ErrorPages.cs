using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Pdf.Storage.Pdf.CustomPages
{
    public class ErrorPages : IErrorPages
    {
        private readonly string _filePath;

        public ErrorPages(IHostingEnvironment env)
        {
            _filePath = $@"{env.ContentRootPath}\Pdf\CustomPages\processing.html";
        }

        public ContentResult PdfIsStillProcessingResponse()
        {
            var content = File.ReadAllText(_filePath);

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html",
                StatusCode = 404
            };
        }
    }
}

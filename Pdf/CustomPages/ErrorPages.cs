using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Pdf.Storage.Pdf.CustomPages
{
    public class ErrorPages : IErrorPages
    {
        private readonly string _filePathForProcessing;
        private readonly string _filePathForNotFound;

        public ErrorPages(IHostingEnvironment env)
        {
            _filePathForProcessing = $@"{env.ContentRootPath}/Pdf/CustomPages/processing.html";
            _filePathForNotFound = $@"{env.ContentRootPath}/Pdf/CustomPages/404.html";
        }

        public ContentResult PdfIsStillProcessingResponse()
        {
            var content = File.ReadAllText(_filePathForProcessing);

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html",
                StatusCode = 404
            };
        }

        public ContentResult PdfNotFoundResponse()
        {
            var content = File.ReadAllText(_filePathForNotFound);

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html",
                StatusCode = 404
            };
        }
    }
}

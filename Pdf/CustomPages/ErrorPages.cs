using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Pdf.Storage.Pdf.CustomPages
{
    public class ErrorPages : IErrorPages
    {
        private readonly string _filePathForProcessing;
        private readonly string _filePathForNotFound;
        private readonly string _filePathForRemoved;

        public ErrorPages(IWebHostEnvironment env)
        {
            _filePathForProcessing = $@"{env.ContentRootPath}/Pdf/CustomPages/processing.html";
            _filePathForNotFound = $@"{env.ContentRootPath}/Pdf/CustomPages/404.html";
            _filePathForRemoved = $@"{env.ContentRootPath}/Pdf/CustomPages/removed.html";
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

        public ContentResult PdfRemovedResponse()
        {
            var content = File.ReadAllText(_filePathForRemoved);

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html",
                StatusCode = 404
            };
        }
    }
}

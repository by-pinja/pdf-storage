using Microsoft.AspNetCore.Mvc;

namespace Pdf.Storage.Pdf.CustomPages
{
    public interface IErrorPages
    {
        ContentResult PdfIsStillProcessingResponse();
        ContentResult PdfNotFoundResponse();
    }
}
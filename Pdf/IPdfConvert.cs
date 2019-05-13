using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public interface IPdfConvert
    {
        byte[] CreatePdfFromHtml(string html, JObject options);
    }
}
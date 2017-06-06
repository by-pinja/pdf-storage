namespace Pdf.Service.Pdf
{
    public interface IPdfConvert
    {
        (byte[] data, string html) CreatePdfFromHtml(string html);
    }
}
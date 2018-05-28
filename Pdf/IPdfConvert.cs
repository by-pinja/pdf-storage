namespace Pdf.Storage.Pdf
{
    public interface IPdfConvert
    {
        (byte[] data, string html) CreatePdfFromHtml(string html, object templateData, object options);
    }
}
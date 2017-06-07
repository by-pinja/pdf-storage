namespace Pdf.Storage.Pdf
{
    public class NewPdfResponse
    {
        public NewPdfResponse(string id, string uri)
        {
            Id = id;
            Uri = uri;
        }

        public string Uri { get; }
        public string Id { get; }
    }
}
namespace Pdf.Storage.Pdf
{
    public class NewPdfResponse
    {
        public NewPdfResponse(string id, string groupId, string pfdUri)
        {
            Id = id;
            GroupId = groupId;
            PfdUri = pfdUri;
        }

        public string PfdUri { get; }
        public string Id { get; }
        public string GroupId { get; }
    }
}
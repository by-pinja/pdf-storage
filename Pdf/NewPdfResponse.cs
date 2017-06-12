using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public class NewPdfResponse
    {
        public NewPdfResponse(string id, string groupId, string pfdUri, object data)
        {
            Id = id;
            GroupId = groupId;
            PfdUri = pfdUri;
            Data = data;
        }

        public string PfdUri { get; }
        public string Id { get; }
        public string GroupId { get; }
        public object Data { get; }
    }
}
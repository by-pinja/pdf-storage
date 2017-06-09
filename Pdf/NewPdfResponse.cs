using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public class NewPdfResponse
    {
        public NewPdfResponse(string id, string groupId, string pfdUri, JObject data)
        {
            Id = id;
            GroupId = groupId;
            PfdUri = pfdUri;
            Data = data;
        }

        public string PfdUri { get; }
        public string Id { get; }
        public string GroupId { get; }
        public JObject Data { get; }
    }
}
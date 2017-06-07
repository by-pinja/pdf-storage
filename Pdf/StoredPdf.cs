namespace Pdf.Storage.Pdf
{
    public class StoredPdf
    {
        public string Group { get; }
        public string Id { get; }
        public byte[] Data { get; }

        public StoredPdf(string group, string id, byte[] data)
        {
            Group = @group;
            Id = id;
            Data = data;
        }
    }
}

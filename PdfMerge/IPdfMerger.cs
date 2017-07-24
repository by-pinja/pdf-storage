namespace Pdf.Storage.PdfMerge
{
    public interface IPdfMerger
    {
        void MergePdf(string entityGroupId, string fileId, MergeRequest[] requests);
    }
}
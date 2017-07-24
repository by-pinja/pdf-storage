namespace Pdf.Storage.PdfMerge
{
    public interface IPdfMerger
    {
        void MergePdf(string entityFileId, MergeRequest[] requests);
    }
}
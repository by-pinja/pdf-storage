using System;
using Pdf.Storage.Data;

namespace Pdf.Storage.Pdf.PdfStores
{
    public class StorageFileId
    {
        public string Group { get; }
        public string Id { get; }
        public string Extension { get; }

        protected StorageFileId() {}

        public StorageFileId(string group, string id, string extension)
        {
            Group = group ?? throw new ArgumentNullException(nameof(group));
            Id = id ?? throw new ArgumentNullException(nameof(id));

            if (extension != "html" && extension != "pdf")
                throw new InvalidOperationException($"Expected extension to be 'pdf' or 'html' but got '{extension}'");

            Extension = extension;
        }

        public StorageFileId(PdfEntity pdfEntity, string extension = "pdf") : this(pdfEntity.GroupId, pdfEntity.FileId, extension)
        {
        }
    }
}
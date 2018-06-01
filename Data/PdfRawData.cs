using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Pdf.Storage.Data
{
    public class PdfRawData
    {
        protected PdfRawData() { }
        public PdfRawData(Guid parentId, string html, object templateData, object options)
        {
            ParentId = parentId;
            Html = html;
            TemplateData = JsonConvert.SerializeObject(templateData);
            Options = JsonConvert.SerializeObject(options);
        }

        public Guid Id { get; protected set; }
        public Guid ParentId { get; protected set; }
        public string Html { get; protected set; }

        [Column(TypeName = "jsonb")]
        public string TemplateData { get; protected set; }

        [Column(TypeName = "jsonb")]
        public string Options { get; protected set; }
    }
}
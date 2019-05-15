using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Data
{
    public class PdfRawDataEntity
    {
        protected PdfRawDataEntity() { }
        public PdfRawDataEntity(Guid parentId, string html, JObject templateData, JObject options)
        {
            ParentId = parentId;
            Html = html;
            TemplateData = templateData;
            Options = options;
        }

        public Guid Id { get; protected set; }
        public Guid ParentId { get; protected set; }
        public string Html { get; protected set; }

        public JObject TemplateData { get; protected set; }
        public JObject Options { get; protected set; }
    }
}
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public static class TemplateDataUtils
    {
        public static ExpandoObject GetTemplateData(object templateData, object rowData)
        {
            var templateDataAsJ = JObject.FromObject(rowData);

            templateDataAsJ.Merge(JObject.FromObject(rowData), 
                new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });

            return templateDataAsJ.ToObject<ExpandoObject>();
        }
    }
}

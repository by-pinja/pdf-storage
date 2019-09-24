using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public static class TemplateUtils
    {
        public static JObject MergeBaseTemplatingWithRows(JObject templateData, JToken rowData)
        {
            templateData.Merge(rowData,
                new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });

            return templateData;
        }
    }
}

using System.Dynamic;
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

        // Theres issue on underlaying print technology, when pdf is printed there is possiblity
        // that page isn't ready before it's printed. For this reason this fix is injected
        // to html page that enforces print to wait until elements are loaded.
        public static string AddWaitForAllPageElementsFixToHtml(string html)
        {
            const string workaroundScript = "<script type=\"text/javascript\">await page.waitFor('*')</script>";
            return html;
        }
    }
}

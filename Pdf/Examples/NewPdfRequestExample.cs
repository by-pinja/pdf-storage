using Newtonsoft.Json.Linq;
using Pdf.Storage.Pdf.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Pdf.Storage.Pdf.Examples
{
    public class NewPdfRequestExample : IExamplesProvider<NewPdfRequest>
    {
        public NewPdfRequest GetExamples()
        {
            return new NewPdfRequest
            {
                Html = @"Text <a href ""http://examplelink"">link</a> {{ header }} {{ row }}",
                BaseData = JObject.FromObject(new
                {
                    header = "header_text_here"
                }),
                RowData = new[] {
                    JObject.FromObject(new { row = "row_data_1" }),
                    JObject.FromObject(new { row = "row_data_2" })
                }
            };
        }
    }
}
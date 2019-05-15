using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Pdf.Dto;
using Protacon.NetCore.WebApi.TestUtil;

namespace Pdf.Storage.Utils.Test
{
    public static class TestExtensions
    {
        public static Task<NewPdfResponse[]> AddPdf(this TestServer host, Guid groupId, int amountOfDataRows = 1, string html = null, object baseData = null)
        {
            var data = Enumerable.Range(0, amountOfDataRows)
                .Select(i => JObject.FromObject(
                    new
                    {
                        content = $"key_for_row_{i}"
                    }))
                .ToArray();

            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = html ?? "<body> {{ header }} {{ content }} </body>",
                        BaseData = JObject.FromObject(
                            baseData ?? new
                            {
                                header = "header_value_here"
                            }),
                        RowData = data
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse[]>()
                .Select();
        }
    }
}
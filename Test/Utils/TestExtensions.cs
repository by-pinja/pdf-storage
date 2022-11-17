using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Test.Dto;
using Protacon.NetCore.WebApi.TestUtil;

namespace Pdf.Storage.Utils.Test
{
    public static class TestExtensions
    {
        public static Task<NewPdfResponseTest[]> AddPdf(this TestServer host, Guid groupId, int amountOfDataRows = 1, string html = null, object baseData = null)
        {
            var data = Enumerable.Range(0, amountOfDataRows)
                .Select(i => JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(
                    new
                    {
                        content = $"key_for_row_{i}"
                    })))
                .ToArray();

            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequestTest
                    {
                        Html = html ?? "<body> {{ header }} {{ content }} </body>",
                        BaseData = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(
                            baseData ?? new
                            {
                                header = "header_value_here"
                            })),
                        RowData = data
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponseTest[]>()
                .Select();
        }
    }
}

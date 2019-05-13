using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Pdf.Dto;
using Protacon.NetCore.WebApi.TestUtil;

namespace Pdf.Storage.Utils.Test
{
    public static class TestExtensions
    {
        public static Task<NewPdfResponse[]> AddPdf(this TestServer host, Guid groupId, int amountOfDataRows = 1)
        {
            var data = Enumerable.Range(0, amountOfDataRows).Select(i => new {
                Key = $"key_for_row_{i}"
            }).ToArray();

            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        BaseData = new {},
                        RowData = data
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse[]>()
                .Select();
        }
    }
}
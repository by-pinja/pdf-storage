using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Pdf.Dto;
using Protacon.NetCore.WebApi.TestUtil;

namespace Pdf.Storage.Utils.Test
{
    public static class TestExtensions
    {
        public static Task<NewPdfResponse[]> AddPdf(this TestServer host, Guid groupId)
        {
            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        BaseData = new {},
                        RowData = new object[] {
                            new {}}
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse[]>()
                .Select();
        }
    }
}
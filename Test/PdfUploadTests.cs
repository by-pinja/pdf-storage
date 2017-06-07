using System;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Pdf;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class PdfUploadTests
    {
        [Fact]
        public void WhenPdfIsUploaded_ThenItCanBeDownloaded()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = host.Post($"/v1/pdf/{groupId}/", new NewPdfRequest
            {
                Html = "<body> {{ TEXT }} </body>",
                Data = JObject.FromObject(new
                {
                    TEXT = "something"
                })
            }).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse>()
                .Select();

            host.Get($"/v1/pdf/{groupId}/{newPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<Byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }
    }
}

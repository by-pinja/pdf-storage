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

            var newPdf = AddPdf(host, groupId);

            host.Get($"/v1/pdf/{groupId}/{newPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact]
        public void WhenFileDoesntExistAtAll_ThenReturn404WithNotAvailableErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            host.Get($"/v1/pdf/{groupId}/{Guid.NewGuid()}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x => x.Should().Match("*404*Invalid*PDF*"));
        }

        [Fact]
        public void WhenFileExistsButIsStillProcessing_ThenReturnProcessingErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            host.Get($"/v1/pdf/{groupId}/{Guid.NewGuid()}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x => x.Should().Match("*404*PDF*is*processing*later*"));
        }

        private static NewPdfResponse AddPdf(TestHost host, Guid groupId)
        {
            return host.Post($"/v1/pdf/{groupId}/", new NewPdfRequest
                {
                    Html = "<body> {{ TEXT }} </body>",
                    Data = JObject.FromObject(new
                    {
                        TEXT = "something"
                    })
                }).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse>()
                .Select();
        }
    }
}

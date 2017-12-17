using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.PdfMerge;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class PdfMergerTests
    {
        [Fact(Skip="Requires fix for https://github.com/HangfireIO/Hangfire/issues/808")]
        public void WhenPdfMergeIsRequested_ThenValidMergeUriIsReturned()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var firstPdf = AddPdf(host, group);
            var secondPdf = AddPdf(host, group);

            var response = host.Post($"v1/merge/{group}", new PdfMergeRequest(firstPdf.Id, secondPdf.Id))
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Passing(x => x.PdfUri.Should().StartWith("http"))
                .Select();

            host
                .WaitForOk(response.PdfUri)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact(Skip="Requires fix for https://github.com/HangfireIO/Hangfire/issues/808")]
        public void WhenPdfFilesAreMerged_ThenMarkOriginalFilesAsOpened()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var firstPdf = AddPdf(host, group);

            host.Post($"v1/merge/{@group}", new PdfMergeRequest(firstPdf.Id))
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Select();

            host.Get($"/v1/usage/{group}/{firstPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountSimpleResponse>()
                .Passing(x =>
                {
                    x.IsOpened.Should().Be(true);
                    x.PdfId.Should().Be(firstPdf.Id);
                });
        }

        [Fact]
        public void WhenZeroPdfsAreDefinedForMerging_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();

            host.Post($"v1/merge/{group}", new PdfMergeRequest() { PdfIds = new string[]{}})
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void WhenDefinedPdfFilesDoesntExist_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();

            host.Post($"v1/merge/{group}", new PdfMergeRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        private NewPdfResponse AddPdf(TestServer host, Guid groupId)
        {
            var pdf =  host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        BaseData = new { BaseKey = "baseKeyValue" },
                        RowData = new object[] {
                            new
                            {
                                Key = "keyHere",
                                TEXT = "something"
                            }}
                    }
                )
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse[]>()
                .Select()
                .Single();

            host.WaitForOk($"{pdf.PdfUri}?noCount=true");

            return pdf;
        }
    }
}

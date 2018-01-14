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

namespace Pdf.Storage.Hangfire
{
    public class PdfMergerTests
    {
        [Fact]
        public void WhenPdfMergeIsRequested_ThenValidMergeUriIsReturned()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var firstPdf = AddPdf(host, group);

            var response = host.Post($"v1/merge/{group}", new PdfMergeRequest(firstPdf.Id))
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Passing(x => x.PdfUri.Should().StartWith("http"))
                .Select();

            host
                .Get(response.PdfUri)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact]
        public void WhenPdfFilesAreMerged_ThenMarkOriginalFilesAsOpened()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var firstPdf = AddPdf(host, group);

            host.Post($"v1/merge/{group}", new PdfMergeRequest(firstPdf.Id))
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Select();

            host.Get($"/v1/usage/{group}/{firstPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().HaveCount(1);
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

            return pdf;
        }
    }
}

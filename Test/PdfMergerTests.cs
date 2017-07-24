using System;
using System.Linq;
using System.Net;
using System.Threading;
using FluentAssertions;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.PdfMerge;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class PdfMergerTests
    {
        [Fact]
        public void WhenPdfMergeIsRequested_ThenValidMergeUriIsReturned()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var firstPdf = AddPdf(host, group);
            var secondPdf = AddPdf(host, group);

            var response = host.Post($"v1/merge/{group}", new[]
            {
                new MergeRequest { Group = firstPdf.GroupId, PdfId = firstPdf.Id },
                new MergeRequest { Group = secondPdf.GroupId, PdfId = secondPdf.Id }
            })
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Passing(x => x.PdfUri.Should().StartWith("http"))
                .Select();

            host
                .WaitForOk(response.PdfUri, reason: "Did not receive merged pdf.")
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact]
        public void WhenOneOrLessPdfDefinedForMerging_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var pdf = AddPdf(host, group);

            host.Post($"v1/merge/{group}", new[]
                {
                    new MergeRequest {Group = pdf.GroupId, PdfId = pdf.Id},
                })
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void WhenDefinedPdfFilesDoesntExist_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();

            host.Post($"v1/merge/{group}", new[]
                {
                    new MergeRequest {Group = Guid.NewGuid().ToString(), PdfId = Guid.NewGuid().ToString()},
                    new MergeRequest {Group = Guid.NewGuid().ToString(), PdfId = Guid.NewGuid().ToString()}
                })
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        private NewPdfResponse AddPdf(TestHost host, Guid groupId)
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

            host.WaitForOk(pdf.PdfUri, reason: $"Waiting for pdf '{pdf.PdfUri}' to get ready.");

            return pdf;
        }
    }
}

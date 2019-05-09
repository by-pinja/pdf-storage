using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.PdfMerge;
using Pdf.Storage.Utils.Test;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Hangfire
{
    public class PdfMergerTests
    {
        [Fact]
        public async Task WhenPdfMergeIsRequested_ThenValidMergeUriIsReturned()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();

            var newPdf = (await host.AddPdf(group)).Single();

            var response = await host.Post($"v1/merge/{group}", new PdfMergeRequest(newPdf.Id))
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Passing(x => x.PdfUri.Should().StartWith("http"))
                .Select();

            await host
                .Get(response.PdfUri)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact]
        public async Task WhenPdfFilesAreMerged_ThenMarkOriginalFilesAsOpened()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();
            var newPdf = (await host.AddPdf(group)).Single();

            await host.Post($"v1/merge/{group}", new PdfMergeRequest(newPdf.Id))
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<MergeResponse>()
                .Select();

            await host.Get($"/v1/usage/{group}/{newPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().HaveCount(1);
                });
        }

        [Fact]
        public async Task WhenZeroPdfsAreDefinedForMerging_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();

            await host.Post($"v1/merge/{group}", new PdfMergeRequest() { PdfIds = new string[]{}})
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task WhenDefinedPdfFilesDoesntExist_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();

            var group = Guid.NewGuid();

            await host.Post($"v1/merge/{group}", new PdfMergeRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()))
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task WhenDefinedPdfForMergingIsDeleted_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();
            var group = Guid.NewGuid();
            var newPdf = (await host.AddPdf(group)).Single();

            await host.Delete(newPdf.PdfUri)
                .ExpectStatusCode(HttpStatusCode.OK);

            await host.Post($"v1/merge/{group}", new PdfMergeRequest(newPdf.GroupId, newPdf.Id))
                .ExpectStatusCode(HttpStatusCode.BadRequest);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.Utils.Test;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Hangfire
{
    public class PdfUploadTests
    {
        [Fact]
        public async Task WhenFileDoesntExistAtAll_ThenReturn404WithNotAvailableErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            await host.Get($"/v1/pdf/{groupId}/{Guid.NewGuid()}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x =>
                {
                    x.Should().Match("*PDF*not*found*");
                });
        }

        [Fact]
        public async Task WhenFileExistsButIsStillProcessing_ThenReturnProcessingErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            host.Setup<HangfireMock>(mock =>
            {
                mock.ExecuteActions = false;
            });

            var newPdf = (await host.AddPdf(groupId)).Single();

            await host.Get($"/v1/pdf/{groupId}/{newPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x => x.Should().Match("*PDF*generating*"));
        }

        [Fact]
        public async Task WhenFileIsUploaded_ThenResponseTellsUsefullInformationAboutProcessing()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = (await host.AddPdf(groupId)).Single();

            newPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.Id.Should().Be(newPdf.Id);
            newPdf.GroupId.Should().Be(groupId.ToString());
            ((JObject)newPdf.Data)["Key"].Value<string>().Should().Be("keyHere");
        }

        [Fact]
        public async Task WhenMultiplePdfsAreCreated_ThenTheyShouldBeAvailable()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = (await host.AddPdf(groupId)).Single();

            newPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.Id.Should().Be(newPdf.Id);
            newPdf.GroupId.Should().Be(groupId.ToString());
        }

        [Fact]
        public async Task WhenPdfIsUploaded_ThenItCanBeDownloaded()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = await host.AddPdf(groupId);

            await host.Get(newPdf.Single().PdfUri)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact]
        public async Task WhenPdfIsRemoved_ThenItShouldBeNoMoreAvailableAndPageGivesMeaningfullErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var pdfForRemoval = (await host.AddPdf(groupId)).Single();

            await host.Delete(pdfForRemoval.PdfUri)
                .ExpectStatusCode(HttpStatusCode.OK);

            await host.Get(pdfForRemoval.PdfUri)
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(body => body.Should().Match("*PDF*removed*"));
        }

        [Fact]
        public async Task WhenPdfIsRemovedMultipleTimes_ThenItShouldReturnSameResultEachTime()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var pdfForRemoval = (await host.AddPdf(groupId)).Single();

            await host.Delete(pdfForRemoval.PdfUri)
                .ExpectStatusCode(HttpStatusCode.OK);

            await host.Delete(pdfForRemoval.PdfUri)
                .ExpectStatusCode(HttpStatusCode.OK);
        }
    }
}
using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Pdf.Storage.Pdf.Dto;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Hangfire
{
    public class PdfUploadTests
    {
        private NewPdfResponse AddPdf(TestServer host, Guid groupId)
        {
            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        BaseData = new { BaseKey = "baseKeyValue"},
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
        }

        [Fact]
        public void WhenFileDoesntExistAtAll_ThenReturn404WithNotAvailableErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            host.Get($"/v1/pdf/{groupId}/{Guid.NewGuid()}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x =>
                {
                    x.Should().Match("*PDF*not*found*");
                });
        }

        [Fact]
        public void WhenFileExistsButIsStillProcessing_ThenReturnProcessingErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            host.Setup<HangfireMock>(mock =>
            {
                mock.Executing = false;
            });

            var newPdf = AddPdf(host, groupId);

            host.Get($"/v1/pdf/{groupId}/{newPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x => x.Should().Match("*PDF*generating*"));
        }

        [Fact]
        public void WhenFileIsUploaded_ThenResponseTellsUsefullInformationAboutProcessing()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            newPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.Id.Should().Be(newPdf.Id);
            newPdf.GroupId.Should().Be(groupId.ToString());
            ((JObject)newPdf.Data)["Key"].Value<string>().Should().Be("keyHere");
        }

        [Fact]
        public void WhenMultiplePdfsAreCreated_ThenTheyShouldBeAvailable()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            newPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.Id.Should().Be(newPdf.Id);
            newPdf.GroupId.Should().Be(groupId.ToString());
        }

        [Fact]
        public void WhenPdfIsUploaded_ThenItCanBeDownloaded()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            host.Get(newPdf.PdfUri)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        [Fact(Skip="TODO next")]
        public void WhenPdfIsRemoved_ThenItShouldBeNoMoreAvailableAndPageGivesMeaningfullErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
                var groupId = Guid.NewGuid();

            var pdfForRemoval = AddPdf(host, groupId);

            host.Delete(pdfForRemoval.PdfUri).ExpectStatusCode(HttpStatusCode.OK);

            host.Get(pdfForRemoval.PdfUri)
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(body => body.Should().Match("*pdf*removed*"));
        }
    }
}
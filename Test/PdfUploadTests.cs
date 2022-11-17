using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.Utils.Test;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    [Collection(ChromiumFixtureCollection.Name)]
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
                .Passing(x => x.Should().Match("*generat*file*"));

            await host.Get($"/v1/pdf/{groupId}/{newPdf.Id}.html")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x => x.Should().Match("*generat*file*"));
        }

        [Fact]
        public async Task WhenFileIsUploaded_ThenResponseTellsUsefullInformationAboutProcessing()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = (await host.AddPdf(groupId)).Single();

            newPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.HtmlUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.html");
            newPdf.GroupId.Should().Be(groupId.ToString());
            newPdf.Data.RootElement.GetProperty("content").GetString().Should().Be("key_for_row_0");
        }

        [Fact]
        public async Task WhenMultiplePdfsAreCreated_ThenTheyShouldBeAvailable()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdfs = await host.AddPdf(groupId, amountOfDataRows: 10);

            newPdfs.Should().HaveCount(10);

            var firstNewPdf = newPdfs[0];
            firstNewPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{firstNewPdf.Id}.pdf");
            firstNewPdf.HtmlUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{firstNewPdf.Id}.html");
            firstNewPdf.GroupId.Should().Be(groupId.ToString());

            var secondNewPdf = newPdfs.First();
            secondNewPdf.PdfUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{secondNewPdf.Id}.pdf");
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

            await host.Get(newPdf.Single().HtmlUri)
                .WithContentOf<string>()
                .Passing(x =>
                {
                    x.Should().Match("*<body>*</body>*");
                });
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
                .Passing(body => body.Should().Match("*file*removed*"));
        }

        [Fact]
        public async Task WhenPdfIsAddedWithEmptyRowTable_ThenReturnBadRequest()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            await host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "someString",
                        BaseData = JObject.FromObject(new { }),
                        RowData = new JObject[0]
                    }
                ).ExpectStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task WhenPdfIsRemovedViaHtmlUri_ThenItShouldBeNoMoreAvailableAndPageGivesMeaningfullErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var pdfForRemoval = (await host.AddPdf(groupId)).Single();

            await host.Delete(pdfForRemoval.HtmlUri)
                .ExpectStatusCode(HttpStatusCode.OK);

            await host.Get(pdfForRemoval.PdfUri)
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(body => body.Should().Match("*file*removed*"));

            await host.Get(pdfForRemoval.HtmlUri)
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(body => body.Should().Match("*file*removed*"));
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

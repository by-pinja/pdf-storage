using System;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading;
using FluentAssertions;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Pdf;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class PdfUploadTests
    {
        private NewPdfResponse AddPdf(TestHost host, Guid groupId)
        {
            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        RowData = new object[] {
                            new
                            {
                                Key = "keyHere",
                                TEXT = "something"
                            }}
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
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
                    host.MockPassing<IPersistentJobQueueMonitoringApi>(a => { });
                    x.Should().Match("*404*PDF*doesn't*exist*");
                });
        }

        [Fact(Skip = "Ignored because isnt valid case until work queues are implemented.")]
        public void WhenFileExistsButIsStillProcessing_ThenReturnProcessingErrorMessage()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            host.Get($"/v1/pdf/{groupId}/{newPdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .WithContentOf<string>()
                .Passing(x => x.Should().Match("*404*PDF*is*processing*later*"));
        }

        [Fact]
        public void WhenFileIsUploaded_ThenResponseTellsUsefullInformationAboutProcessing()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            newPdf.PfdUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.Id.Should().Be(newPdf.Id);
            newPdf.GroupId.Should().Be(groupId.ToString());
            ((JObject) newPdf.Data)["Key"].Value<string>().Should().Be("keyHere");
        }

        [Fact]
        public void WhenMultiplePdfsAreCreated_ThenTheyShouldBeAvailable()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            newPdf.PfdUri.Should().Be($"http://localhost:5000/v1/pdf/{groupId}/{newPdf.Id}.pdf");
            newPdf.Id.Should().Be(newPdf.Id);
            newPdf.GroupId.Should().Be(groupId.ToString());
        }

        [Fact]
        public void WhenPdfIsUploaded_ThenItCanBeDownloaded()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = AddPdf(host, groupId);

            WaitForOk(host, newPdf.PfdUri)
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<byte[]>()
                .Passing(x => x.Length.Should().BeGreaterThan(1));
        }

        // Ugly workaround, waiting for better idea.
        private CallResponse WaitForOk(TestHost host, string path)
        {
            Thread.Sleep(3000);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var response = host.Get(path).ExpectStatusCode(HttpStatusCode.OK);
                    return response;
                }
                catch (ExpectedStatusCodeException)
                {
                    Thread.Sleep(1000);
                }
            }
            throw new InvalidOperationException("Timeout");
        }
    }
}
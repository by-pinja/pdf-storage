using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Dto;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class PdfOpenCountTests
    {
        [Fact]
        public void WhenPdfIsOpened_ThenInformationAboutOpeningIsQueruable()
        {
            var host = TestHost.Run<TestStartup>();
            var group = "default";

            var pdf = AddPdf(host, group);

            host.WaitForOk($"{pdf.PdfUri}");

            host.Get($"/v1/usage/{group}/")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<IEnumerable<PdfUsageCountSimpleResponse>>()
                .Passing(x =>
                {
                    x.Should().HaveCount(1);
                    x.Single().IsOpened.Should().BeTrue();
                    x.Single().PdfId.Should().Be(pdf.Id);
                });

            host.Get($"/v1/usage/{group}/{pdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().HaveCount(1);
                    x.Opened.Single().Should().BeCloseTo(DateTime.UtcNow, precision: 1000);
                });
        }

        [Fact]
        public void WhenPdfIsOpenedWithNoCountQuery_ThenDontCountIsAsOpened()
        {
            var host = TestHost.Run<TestStartup>();
            var group = Guid.NewGuid().ToString();

            var pdf = AddPdf(host, group);

            host.WaitForOk($"{pdf.PdfUri}?noCount=true");

            host.Get($"/v1/usage/{group}/")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<IEnumerable<PdfUsageCountSimpleResponse>>()
                .Passing(x => x.Should().HaveCount(0));

            host.Get($"/v1/usage/{group}/{pdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x => x.Opened.Should().HaveCount(0));
        }

        private NewPdfResponse AddPdf(TestHost host, string groupId)
        {
            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        RowData = new object[] {
                            new {}}
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse[]>()
                .Select()
                .Single();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.Utils.Test;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    [Collection(ChromiumFixtureCollection.Name)]
    public class PdfOpenCountTests
    {
        [Fact]
        public async Task WhenPdfIsOpened_ThenInformationAboutOpeningIsQueruable()
        {
            var host = TestHost.Run<TestStartup>();
            var group = Guid.NewGuid();

            var pdf = (await host.AddPdf(group)).Single();

            await host.Get($"{pdf.PdfUri}")
                .ExpectStatusCode(HttpStatusCode.OK);

            await host.Get($"/v1/usage/{group}/")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfGroupUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().Be(1);
                    x.Total.Should().Be(1);
                });

            await host.Get($"/v1/usage/{group}/{pdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().HaveCount(1);
                    x.Opened.Single().Should().BeCloseTo(DateTime.UtcNow, precision: new TimeSpan(0, 0, 5));
                });
        }

        [Fact]
        public async Task WhenPdfIsOpenedWithNoCountQuery_ThenDontCountIsAsOpened()
        {
            var host = TestHost.Run<TestStartup>();
            var group = Guid.NewGuid();

            var pdf = (await host.AddPdf(group)).Single();

            await host.Get($"{pdf.PdfUri}?noCount=true");

            await host.Get($"/v1/usage/{group}/")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfGroupUsageCountResponse>()
                .Passing(x =>
                {
                    x.Total.Should().Be(1);
                    x.Opened.Should().Be(0);
                });

            await host.Get($"/v1/usage/{group}/{pdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x => x.Opened.Should().HaveCount(0));
        }
    }
}

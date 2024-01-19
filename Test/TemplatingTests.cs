using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Utils.Test;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    [Collection(ChromiumFixtureCollection.Name)]
    public class TemplatingTests
    {
        [Fact]
        public async Task WhenHtmlWithMustacheTemplateIsSent_ThenApplyTemplating()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = (await host.AddPdf(groupId)).Single();

            await host.Get(newPdf.HtmlUri)
                .WithContentOf<string>()
                .Passing(x =>
                {
                    x.Should().Match("*header_value_here*key_for_row_0*");
                });
        }
    }
}

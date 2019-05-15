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

        [Fact]
        public async Task WhenBarcodeTranslationIsApplied_ThenApplyTranslationAndUpdateTemplate()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = (await host
                .AddPdf(groupId,
                    html: "<img src=\"{{ barcode }}\"/>",
                    baseData: new { barcode = "[translate:barcode]AE5C9B"}))
                .Single();

            await host.Get(newPdf.HtmlUri)
                .WithContentOf<string>()
                .Passing(x =>
                {
                    x.Should().Match("*<img src=\"data:image/png;base64,*\"/>*");
                });
        }

        [Fact]
        public async Task WhenBarcodeTranslationWithOptionsIsApplied_ThenApplyTranslationToTemplatesWithDefinedOptions()
        {
            var host = TestHost.Run<TestStartup>();
            var groupId = Guid.NewGuid();

            var newPdf = (await host
                .AddPdf(groupId,
                    html: "<img src=\"{{ barcode }}\"/>",
                    baseData: new { barcode = "[translate:barcode:{includeText: true, foregroundColor: '#4286f4'}]AE5C9B"}))
                .Single();

            await host.Get(newPdf.HtmlUri)
                .WithContentOf<string>()
                .Passing(x =>
                {
                    // Note here: It's bit tricky to properly test that image matches expectation for options.
                    // It's possible that this test will cause false positive on depency update of libraries if image generation changes:
                    // if so manually validate image as correct one and update this test again to match expectations.
                    // This is here to alert possible error if something have changed for image generation.
                    x.Should().Match("*<img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUh*\"/>*");
                });
        }
    }
}
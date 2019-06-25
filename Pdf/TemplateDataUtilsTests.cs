using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Pdf.Storage.Pdf
{
    public class TemplateUtilsTests
    {
        [Fact]
        public void WhenTemplateIsMergedToRow_ThenOutputContainsBoth()
        {
            var result = TemplateUtils.MergeBaseTemplatingWithRows(JObject.FromObject(new { Prop1 = 1 }), JObject.FromObject(new { Prop2 = 2 }));
            result["Prop1"].Value<int>().Should().Be(1);
            result["Prop2"].Value<int>().Should().Be(2);
        }

        [Fact]
        public void WhenTemplateIsMergedToRowWithConflictingData_ThenOutputContainsRowData()
        {
            var result = TemplateUtils.MergeBaseTemplatingWithRows(JObject.FromObject(new { Prop1 = 1 }), JObject.FromObject(new { Prop1 = 2 }));
            result["Prop1"].Value<int>().Should().Be(2);
        }

        [Fact]
        public void WhenWaitForAllElementsAreAddedForEmptyDocument_DontDoAnything()
        {
            var result = TemplateUtils.AddWaitForAllPageElementsFixToHtml("");
            result.Should().Be("");
        }

        [Fact]
        public void WhenTheresContentAddWorkaroundScriptToEnd()
        {
            var result = TemplateUtils.AddWaitForAllPageElementsFixToHtml("some random content <a hred='..'></a>");
            result.Should().Match("some random content <a hred='..'></a><script type=\"text/javascript\">await page.waitFor('*')</script>");
        }
    }
}
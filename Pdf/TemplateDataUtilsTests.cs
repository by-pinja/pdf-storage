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
        public void WhenDataIsMerged_ReturnCopyInsteadOfModifyingExisting()
        {
            var inputJObject = JObject.FromObject(new { Prop1 = 1 });
            var result = TemplateUtils.MergeBaseTemplatingWithRows(inputJObject, JObject.FromObject(new { Prop1 = 2 }));
            result["Prop1"].Value<int>().Should().Be(2);
            inputJObject["Prop1"].Value<int>().Should().Be(1);
        }
    }
}
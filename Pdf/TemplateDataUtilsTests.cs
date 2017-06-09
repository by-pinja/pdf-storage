using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Pdf.Storage.Pdf
{
    public class TemplateDataUtilsTests
    {
        [Fact]
        public void WhenTemplateIsMergedToRow_ThenOutputContainsBoth()
        {
            var result = TemplateDataUtils.GetTemplateData(new {Prop1 = 1}, new {Prop2 = 2});
            result.Single(x => x.Key == "Prop1").Value.Should().Be(1);
            result.Single(x => x.Key == "Prop2").Value.Should().Be(2);
        }

        [Fact]
        public void WhenTemplateIsMergedToRowWithConflictingData_ThenOutputContainsRowData()
        {
            var result = TemplateDataUtils.GetTemplateData(new { Prop1 = 1 }, new { Prop1 = 2 });
            result.Single(x => x.Key == "Prop1").Value.Should().Be(2);
        }
    }
}
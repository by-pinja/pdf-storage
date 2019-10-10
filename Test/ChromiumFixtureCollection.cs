using Pdf.Storage.Test.Utils;
using Xunit;

namespace Pdf.Storage.Test
{
    [CollectionDefinition(Name)]
    public class ChromiumFixtureCollection : ICollectionFixture<ChromiumFixture>
    {
        public const string Name = nameof(ChromiumFixtureCollection);
    }
}
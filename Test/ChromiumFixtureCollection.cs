using Pdf.Storage.Test.Utils;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = -1)]

namespace Pdf.Storage.Test
{
    [CollectionDefinition(Name)]
    public class ChromiumFixtureCollection : ICollectionFixture<ChromiumFixture>
    {
        public const string Name = nameof(ChromiumFixtureCollection);
    }
}
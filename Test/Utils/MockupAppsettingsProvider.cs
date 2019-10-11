using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Pdf.Storage.Test.Utils
{
    public class MockupAppsettingsProvider : ConfigurationProvider
    {
        public Dictionary<string, string> Configurations { get; } = new Dictionary<string, string>();

        public override void Load()
        {
            Data = Configurations;
        }

        private class MockupAppsettingsSource : IConfigurationSource
        {
            private readonly MockupAppsettingsProvider _provider;

            public MockupAppsettingsSource(MockupAppsettingsProvider provider)
            {
                _provider = provider;
            }

            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                return _provider;
            }
        }

        public IConfigurationSource GetSource()
        {
            return new MockupAppsettingsSource(this);
        }
    }
}
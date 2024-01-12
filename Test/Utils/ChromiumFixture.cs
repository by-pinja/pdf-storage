using System;
using PuppeteerSharp;

namespace Pdf.Storage.Test.Utils
{
    public class ChromiumFixture : IDisposable
    {
        public ChromiumFixture()
        {
            new BrowserFetcher().DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId).Wait();
        }

        public void Dispose()
        {
        }
    }
}

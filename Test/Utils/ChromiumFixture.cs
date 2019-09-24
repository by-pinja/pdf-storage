using System;
using PuppeteerSharp;

namespace Pdf.Storage.Test.Utils
{
    public class ChromiumFixture : IDisposable
    {
        public ChromiumFixture()
        {
            new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).Wait();
        }

        public void Dispose()
        {
        }
    }
}
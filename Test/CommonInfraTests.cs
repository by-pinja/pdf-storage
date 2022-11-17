using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Pdf.Storage.Hangfire;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    [Collection(ChromiumFixtureCollection.Name)]
    public class CommonInfraTests
    {
        [Fact]
        public async Task WhenHostIsStarted_ThenSwaggerOpenApiJsonIsAvailable()
        {
            var host = TestHost.Run<TestStartup>();

            await host.Get("swagger/v1/swagger.json")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<JsonDocument>();
        }
    }
}

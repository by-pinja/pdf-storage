using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Hangfire;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class CommonInfraTests
    {
        [Fact]
        public async Task WhenHostIsStarted_ThenSwaggerOpenApiJsonIsAvailable()
        {
            var host = TestHost.Run<TestStartup>();

            await host.Get("swagger/v1/swagger.json")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<JObject>();
        }
    }
}
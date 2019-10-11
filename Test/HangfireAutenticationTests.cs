using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Test.Utils;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Test
{
    public class HangfireAutenticationTests
    {
        [Fact]
        public async Task WhenHangfireDashboardIsAccessedWithPasswordSet_ThenReturnBasicAuthChallenge()
        {
            var host = TestHost
                .Run<TestStartup>()
                .Setup<MockupAppsettingsProvider>(settings =>
                {
                    settings.Configurations.Add("Hangfire:AllowedIpAddresses:0", "*");
                    settings.Configurations.Add("Hangfire:DashboardUser", "foo");
                    settings.Configurations.Add("Hangfire:DashboardPassword", "bar");
                });

            await host.Get($"/hangfire")
                .ExpectStatusCode(HttpStatusCode.Unauthorized)
                .WithContentOf<string>()
                .Passing(x => x.Should().Contain("Authentication required"));
        }

        [Fact]
        public async Task WhenHangfireDashboardIsAccessedWithValidBasicAuthHeader_ThenAllowAccess()
        {
            string basicAuthHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes("foo:bar"));

            var host = TestHost
                .Run<TestStartup>()
                .Setup<MockupAppsettingsProvider>(settings =>
                {
                    settings.Configurations.Add("Hangfire:AllowedIpAddresses:0", "*");
                    settings.Configurations.Add("Hangfire:DashboardUser", "foo");
                    settings.Configurations.Add("Hangfire:DashboardPassword", "bar");
                });

            await host
                .Get($"/hangfire", headers: new Dictionary<string, string> { { "Authorization", $"Basic {basicAuthHeader}" } })
                .ExpectStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task WhenNotValidIpAddressIsntConfigured_ThenReturnUnAuthorized()
        {
            var host = TestHost
                .Run<TestStartup>()
                .Setup<MockupAppsettingsProvider>(settings =>
                {
                    // Actually during these tests remote ip address is not available (tests doesnt use network)
                    // so only way to pass ip limit middleware is to add "*" any ip address is allowed during tests.
                    // When ip is configured then this should return 401
                    settings.Configurations.Add("Hangfire:AllowedIpAddresses:0", "1.2.3.4");
                });

            await host
                .Get($"/hangfire")
                .ExpectStatusCode(HttpStatusCode.Unauthorized);
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pdf.Storage.Config;

namespace Pdf.Storage.Hangfire
{
    public class IpWhitelistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsSnapshot<HangfireConfig> _hangfireConfig;

        public IpWhitelistMiddleware(RequestDelegate next, IOptionsSnapshot<HangfireConfig> commonConfig)
        {
            _next = next;
            _hangfireConfig = commonConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "remote_ip_address_not_available";

            var allowedIpAddresses = _hangfireConfig.Value.AllowedIpAddresses;

            if (IsRemoteIpAddressLocalHost(remoteIp, context) ||
                allowedIpAddresses.Contains("*") ||
                allowedIpAddresses.Any(x => x == remoteIp))
            {
                await _next.Invoke(context);
                return;
            }

            await Unauthorized(context);
        }

        private static async Task Unauthorized(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"Client ip not allowed ({context.Connection.RemoteIpAddress})");
        }

        private static bool IsRemoteIpAddressLocalHost(string remoteIp, HttpContext context)
        {
            return remoteIp == "127.0.0.1" ||
                remoteIp == "::1" ||
                remoteIp == context.Connection.LocalIpAddress?.ToString();
        }
    }
}
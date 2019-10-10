using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;

namespace Pdf.Storage.Hangfire
{
    public class HangfireIpBasedAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IEnumerable<string> _allowedIpAddresses;

        public HangfireIpBasedAuthorizationFilter(IOptions<CommonConfig> config)
        {
            _allowedIpAddresses = config.Value.AllowedIpAddresses;
        }

        public bool Authorize(DashboardContext context)
        {
            if (_allowedIpAddresses.Contains("*"))
                return true;

            if (string.IsNullOrEmpty(context.Request.RemoteIpAddress))
                return false;

            if (context.Request.RemoteIpAddress == "127.0.0.1" || context.Request.RemoteIpAddress == "::1" || _allowedIpAddresses.Any(x => x == context.Request.RemoteIpAddress))
                return true;

            if (context.Request.RemoteIpAddress == context.Request.LocalIpAddress)
                return true;

            return false;
        }
    }
}
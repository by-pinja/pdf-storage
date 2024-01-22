using System;
using System.Linq;
using System.Net.Http.Headers;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Pdf.Storage.Hangfire;

public class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    public HangfireBasicAuthenticationFilter(IOptions<HangfireConfiguration> hangfireConfig)
    {
        _hangfireConfig = hangfireConfig;
    }

    private record Tokens
    {
        private readonly string[] _tokens;
        public string Username => _tokens[0];
        public string Password => _tokens[1];

        public Tokens(string[] tokens)
        {
            _tokens = tokens;
        }

        public bool IsInvalid()
        {
            return HasTwoTokens() && IsValidToken(Username) && IsValidToken(Password);
        }

        public bool MatchingCredentials(string? user, string? pass)
        {
            return user != null && pass != null && Username.Equals(user) && Password.Equals(pass);
        }

        private static bool IsValidToken(string token)
        {
            return string.IsNullOrWhiteSpace(token);
        }

        private bool HasTwoTokens()
        {
            return _tokens.Length == 2;
        }
    }

    private const string AuthenticationScheme = "HangfireBasic";
    private readonly IOptions<HangfireConfiguration> _hangfireConfig;

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"];

        if(IpIsLocalhost(httpContext))
            return true;

        if(!IpAddressIsAllowed(httpContext))
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.WriteAsync("IP address not allowed.");
            return false;
        }

        if (!HasAuthHeader(header))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        var authValues = AuthenticationHeaderValue.Parse(header);

        if (IsNotBasicAuthentication(authValues))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        var tokens = GetAuthenticationTokens(authValues);

        if (tokens.IsInvalid())
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        if (tokens.MatchingCredentials(_hangfireConfig.Value.DashboardUser, 
            _hangfireConfig.Value.DashboardPassword))
        {
            return true;
        }

        SetChallengeResponse(httpContext);
        return false;
    }

    private static bool HasAuthHeader(StringValues header)
    {
        return !string.IsNullOrWhiteSpace(header);
    }

    private static Tokens GetAuthenticationTokens(AuthenticationHeaderValue authValues)
    {
        var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
        var parts = parameter.Split(':');
        return new Tokens(parts);
    }

    private bool IpAddressIsAllowed(HttpContext? httpContext)
    {
        var remoteIpAddress = GetRemoteIpAddress(httpContext);
        var allowedIpAddresses = _hangfireConfig.Value.AllowedIpAddresses.ToList();

        return allowedIpAddresses.Contains("*") || allowedIpAddresses.Any(ip => ip == remoteIpAddress);
    }

    private static string GetRemoteIpAddress(HttpContext? httpContext)
    {
        return httpContext?.Connection.RemoteIpAddress?.ToString() ?? "remote_ip_address_not_available";
    }

    private static bool IpIsLocalhost(HttpContext? httpContext)
    {
        var remoteIpAddress = GetRemoteIpAddress(httpContext);
        return remoteIpAddress == "127.0.0.1" || remoteIpAddress == "::1" || remoteIpAddress == "localhost";
    }

    private static bool IsNotBasicAuthentication(AuthenticationHeaderValue authValues)
    {
        return !AuthenticationScheme.Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase);
    }

    private static void SetChallengeResponse(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        httpContext.Response.WriteAsync("Authentication is required.");
    }
}

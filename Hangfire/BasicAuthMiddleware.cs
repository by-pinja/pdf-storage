using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pdf.Storage.Config;

namespace Pdf.Storage.Hangfire
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsSnapshot<HangfireConfig> _commonConfig;

        public BasicAuthMiddleware(RequestDelegate next, IOptionsSnapshot<HangfireConfig> commonConfig)
        {
            _next = next;
            _commonConfig = commonConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            var validUsername = _commonConfig.Value.DashboardUser;
            var validPassword = _commonConfig.Value.DashboardPassword;

            if (string.IsNullOrEmpty(validUsername) || string.IsNullOrEmpty(validPassword))
            {
                await _next.Invoke(context);
                return;
            }

            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];

                if (IsAuthorized(username, validUsername, password, validPassword))
                {
                    await _next.Invoke(context);
                    return;
                }
            }

            // Return authentication type (causes browser to show login dialog)
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"Authentication required.");
        }

        private bool IsAuthorized(string username, string validUserName, string password, string validPassword)
        {
            return username.Equals(validUserName, StringComparison.InvariantCultureIgnoreCase) && password.Equals(validPassword);
        }
    }
}
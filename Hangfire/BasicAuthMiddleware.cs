using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Pdf.Storage.Hangfire
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<CommonConfig> _commonConfig;
        private readonly string _userName;
        private readonly string _password;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<CommonConfig> commonConfig)
        {
            _next = next;
            _commonConfig = commonConfig;
            _userName = commonConfig.Value.HangfireDashboardUser;
            _password = commonConfig.Value.HangfireDashboardPassword;
        }

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_password))
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

                if (IsAuthorized(username, password))
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

        public bool IsAuthorized(string username, string password)
        {
            return username.Equals(_userName, StringComparison.InvariantCultureIgnoreCase) && password.Equals(_password);
        }
    }
}
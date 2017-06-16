using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;

namespace Pdf.Storage.Test.Utils
{
    public class TestAuthenticationMiddlewareForApiKey : AuthenticationMiddleware<TestAuthenticationMiddlewareForApiKey.TestAuthenticationOptionsForApiKey>
    {
        public TestAuthenticationMiddlewareForApiKey(RequestDelegate next, IOptions<TestAuthenticationOptionsForApiKey> options, ILoggerFactory loggerFactory)
            : base(next, options, loggerFactory, new UrlTestEncoder())
        {
        }

        protected override AuthenticationHandler<TestAuthenticationOptionsForApiKey> CreateHandler()
        {
            return new TestAuthenticationHandlerForApiKey();
        }

        public class TestAuthenticationOptionsForApiKey : AuthenticationOptions
        {
            public virtual ClaimsIdentity Identity { get; } = new ClaimsIdentity(new[]
            {
                new Claim("apikey", Guid.NewGuid().ToString()),
            }, "test");

            public TestAuthenticationOptionsForApiKey()
            {
                AuthenticationScheme = "ApiKey";
            }
        }

        private class TestAuthenticationHandlerForApiKey : AuthenticationHandler<TestAuthenticationOptionsForApiKey>
        {
            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var authenticationTicket = new AuthenticationTicket(
                    new ClaimsPrincipal(Options.Identity),
                    new AuthenticationProperties(),
                    Options.AuthenticationScheme);

                return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
            }
        }
    }
}
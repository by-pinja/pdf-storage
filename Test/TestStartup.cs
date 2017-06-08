using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pdf.Storage.Test
{
    public class TestStartup
    {
        private readonly Startup _original;

        public TestStartup(IHostingEnvironment env)
        {
            _original = new Startup(env);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _original.ConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _original.Configure(app, env, loggerFactory);
        }
    }
}

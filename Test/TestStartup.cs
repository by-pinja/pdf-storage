using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;
using Protacon.NetCore.WebApi.TestUtil;

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

            services
                .RemoveService<PdfDataContext>()
                .RemoveService<DbContextOptions<PdfDataContext>>()
                .AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseHangfireServer();
            app.UseMvc();
        }
    }
}

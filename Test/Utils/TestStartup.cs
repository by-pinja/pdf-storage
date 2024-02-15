using System;
using System.IO;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pdf.Storage.Config;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Test.Utils;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;

namespace Pdf.Storage.Hangfire
{
    public class TestStartup
    {
        public TestStartup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();

            services.AddAuthentication()
                .AddDisabledApiKeyAuth();

            services.AddCommonAppServices();

            services.AddTransient<IMqMessages, MqMessagesNullObject>();
            services.AddSingleton<IStorage, InMemoryPdfStorage>();

            services.AddSwaggerGenConfiguration();

            services.AddSingleton<IHangfireQueue>(provider =>
            {
                return new HangfireMock(provider);
            });

            services.AddSingleton(provider =>
            {
                return (HangfireMock)provider.GetService<IHangfireQueue>();
            });

            var dbId = Guid.NewGuid().ToString();
            services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase(dbId));

            services.AddHangfire(config => config.UseMemoryStorage());

            var mockupAppsettingsProvider = new MockupAppsettingsProvider();
            services.AddSingleton(mockupAppsettingsProvider);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                .AddEnvironmentVariables()
                .Add(mockupAppsettingsProvider.GetSource())
                .Build();

            services.Configure<CommonConfig>(configuration);

            services.Configure<HangfireConfig>(configuration.GetSection("Hangfire"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                var config = app.ApplicationServices.GetRequiredService<IOptions<HangfireConfiguration>>();

                endpoints.MapHangfireDashboard("/hangfire", options: new DashboardOptions
                {
                    Authorization = [new HangfireBasicAuthenticationFilter(config)],
                });

                endpoints.MapControllers();
            });
        }
    }
}

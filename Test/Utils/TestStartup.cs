using System;
using System.IO;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pdf.Storage.Config;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Test.Utils;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;

namespace Pdf.Storage.Hangfire
{
    public class TestStartup
    {
        public TestStartup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()))
                    .AddNewtonsoftJson();

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
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            app.Map("/hangfire", appBuilder =>
            {
                appBuilder.UseMiddleware<IpWhitelistMiddleware>();
                appBuilder.UseMiddleware<BasicAuthMiddleware>();
                appBuilder.UseHangfireDashboard("", new DashboardOptions
                {
                    Authorization = new IDashboardAuthorizationFilter[] { }
                });
            });

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}

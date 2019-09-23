using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
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
            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));

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

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                .Build();

            services.Configure<CommonConfig>(configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseSwagger();
            app.UseMvc();
        }
    }
}

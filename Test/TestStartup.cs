using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;

namespace Pdf.Storage.Test
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

            services.AddNodeServices();

            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddTransient<IPdfStorage, GoogleCloudPdfStorage>();
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();
            services.AddTransient<Uris>();
            services.AddTransient<IMqMessages, MqMessagesNullObject>();

            var dbId = Guid.NewGuid().ToString();
            services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase(dbId));

            services.AddSingleton<IPdfStorage, InMemoryPdfStorage>();

            services.AddTransient<IPdfMerger, PdfMerger>();

            services.AddHangfire(config => config.UseMemoryStorage());

            services.Configure<AppSettings>(a => a.BaseUrl = "http://localhost:5000");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseAuthentication();

            // Workaround for hanfire instability issue during testing.
            Retry.Action(() => app.UseHangfireServer(), retryInterval: TimeSpan.FromMilliseconds(100), maxAttemptCount: 5);

            app.UseMvc();
        }
    }
}

using System;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pdf.Storage.Data;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
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

            services.AddNodeServices();

            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddTransient<IPdfStorage, GoogleCloudPdfStorage>();
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();
            services.AddTransient<Uris>();
            services.AddTransient<IMqMessages, MqMessagesNullObject>();

            services.AddSingleton<IHangfireQueue>(provider => {
                return new HangfireMock(provider);
            });

            services.AddSingleton<HangfireMock>(provider => {
                return (HangfireMock)provider.GetService<IHangfireQueue>();
            });

            var dbId = Guid.NewGuid().ToString();
            services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase(dbId));

            services.AddSingleton<IPdfStorage, InMemoryPdfStorage>();

            services.AddTransient<IPdfMerger, PdfMerger>();

            services.Configure<AppSettings>(a => a.BaseUrl = "http://localhost:5000");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseAuthentication();

            // Workaround for hanfire instability issue during testing.

            app.UseMvc();
        }
    }
}

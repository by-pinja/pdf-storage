using System.IO;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Pdf.Storage.Data;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.Test;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Swashbuckle.AspNetCore.Swagger;

namespace Pdf.Storage
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddJsonFile("appsettings.secrets.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));

            services.AddNodeServices();

            services.AddSwaggerGen(c =>
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;

                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Pdf.Storage",
                        Version = "v1",
                        Description = File.ReadAllText(Path.Combine(basePath, "ApiDescription.md"))
                    });

                c.OperationFilter<ApplyApiKeySecurityToDocument>();
            });
            services.Configure<AppSettings>(Configuration);

            services.AddDbContext<PdfDataContext>(opt => 
                opt.UseNpgsql(Configuration["ConnectionString"]));

            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddTransient<IPdfStorage, GoogleCloudPdfStorage>();
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();

            services.Configure<ApiKeyAuthenticationOptions>(Configuration.GetSection("ApiAuthentication"));

            services.AddHangfire(config => config.UsePostgreSqlStorage(Configuration["ConnectionString"]));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
            }

            app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

            MigrateDb(app);

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pdf.Storage");
                c.RoutePrefix = "doc";
            });

            app.UseHangfireServer();
            app.UseHangfireDashboard();

            app.UseMvc();
        }

        private static void MigrateDb(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.GetService<PdfDataContext>();
            context.Database.Migrate();
        }
    }
}

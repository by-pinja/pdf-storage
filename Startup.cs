using System;
using System.IO;
using System.Linq;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;
using Swashbuckle.AspNetCore.Swagger;
using Hangfire.PostgreSql;
using Amazon.S3;

namespace Pdf.Storage
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddApiKeyAuth(options =>
                {
                    if(Configuration.GetChildren().All(x => x.Key != "ApiAuthentication"))
                        throw new InvalidOperationException($"Expected 'ApiAuthentication' section.");

                    var keys = Configuration.GetSection("ApiAuthentication:Keys")
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value);

                    options.Keys = keys;
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

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

            if (bool.Parse(Configuration["Mock:Db"] ?? "false"))
            {
                var dbId = Guid.NewGuid().ToString();

                services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase(dbId));

                services.AddHangfire(config => config.UseMemoryStorage());
            }
            else
            {
                services.AddDbContext<PdfDataContext>(opt =>
                    opt.UseNpgsql(Configuration["connectionString"]));

                services.AddHangfire(config =>
                    config.UsePostgreSqlStorage(Configuration["connectionString"] ?? throw new InvalidOperationException("Missing: ConnectionString")));

            }

            if (bool.Parse(Configuration["Mock:GoogleBucket"] ?? "false"))
            {
                services.AddSingleton<IPdfStorage, InMemoryPdfStorage>();
            }
            else
            {
                services.AddTransient<IPdfStorage, GoogleCloudPdfStorage>();
            }

            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();
            services.AddTransient<IPdfMerger, PdfMerger>();
            services.AddTransient<Uris>();
            services.AddTransient<IHangfireQueue, HangfireQueue>();


            if (bool.Parse(Configuration["Mock:Mq"] ?? "false"))
            {
                services.AddTransient<IMqMessages, MqMessagesNullObject>();
            }
            else
            {
                services.AddTransient<IMqMessages, MqMessages>();
            }

            switch(Configuration["PdfStoreType"] ?? "google")
            {
                case "aws":
                    services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
                    services.AddAWSService<IAmazonS3>();
                    break;
                case "google":
                    services.AddTransient<IPdfStorage, GoogleCloudPdfStorage>();
                    break;
                case "inMemory":
                    services.AddSingleton<IPdfStorage, InMemoryPdfStorage>();
                    break;
                default:
                    throw new InvalidOperationException("Invalid configuration: PdfStoreType");
            }

            services.Configure<ApiKeyAuthenticationOptions>(Configuration.GetSection("ApiAuthentication"));
            services.Configure<MqConfig>(Configuration.GetSection("Mq"));
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("CorsPolicy");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
            }

            app.UseAuthentication();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pdf.Storage");
                c.RoutePrefix = "doc";
            });

            switch(GetAppRole())
            {
                case "api":
                    app.UseMvc();
                    break;
                case "worker":
                    app.UseHangfireServer();
                    break;
                default:
                    app.UseMvc();
                    app.UseHangfireServer();
                    break;
            }

            app.UseHangfireDashboard();
        }

        private string GetAppRole()
        {
            return Configuration["AppRole"] ?? "standalone";
        }
    }
}

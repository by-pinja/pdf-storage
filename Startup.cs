﻿using System;
using System.Linq;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Hangfire.PostgreSql;
using Pdf.Storage.Config;
using Pdf.Storage.Pdf.Config;
using Pdf.Storage.Pdf.PdfStores;
using Pdf.Storage.Migrations;
using Hangfire.Dashboard;
using System.IO.Abstractions;
using Microsoft.Extensions.Options;

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
            services.AddApplicationInsightsTelemetry();

            services.AddAuthentication()
                .AddApiKeyAuth(options =>
                {
                    if (Configuration.GetChildren().All(x => x.Key != "ApiAuthentication"))
                        throw new InvalidOperationException($"Expected 'ApiAuthentication' section.");

                    var keys = Configuration.GetSection("ApiAuthentication:Keys")
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value);

                    options.ValidApiKeys = keys;
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddMvc().AddNewtonsoftJson();

            services.AddSwaggerGenConfiguration();

            services.Configure<CommonConfig>(Configuration);

            services.AddOptions<HangfireConfiguration>()
                .Bind(Configuration.GetSection("Hangfire"))
                .ValidateDataAnnotations();

            services.AddCommonAppServices();

            services.AddTransient<IHangfireQueue, HangfireQueue>();

            services.AddHostedService<ApplicationInsightsTelemetryBackgroundService>();

            switch (Configuration["DbType"])
            {
                case "inMemory":
                    var dbId = Guid.NewGuid().ToString();
                    services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase(dbId));
                    services.AddHangfire(config => config.UseMemoryStorage());
                    break;
                case "postreSql":
                    services.AddDbContext<NpSqlDataContextForMigrations>(opt =>
                        opt.UseNpgsql(Configuration["ConnectionString"]));

                    services.AddDbContext<PdfDataContext>(opt =>
                        opt.UseNpgsql(Configuration["ConnectionString"]));

                    services.AddHangfire(config =>
                        config
                            .UseFilter(new PreserveOriginalQueueAttribute())
                            .UsePostgreSqlStorage(Configuration["ConnectionString"] ?? throw new InvalidOperationException("Missing: ConnectionString")));
                    break;
                case "sqlServer":
                    services.AddDbContext<MsSqlDataContextForMigrations>(opt =>
                            opt.UseSqlServer(Configuration["ConnectionString"]));

                    services.AddDbContext<PdfDataContext>(opt =>
                            opt.UseSqlServer(Configuration["ConnectionString"]));

                    services.AddHangfire(config =>
                        config
                            .UseFilter(new PreserveOriginalQueueAttribute())
                            .UseSqlServerStorage(Configuration["ConnectionString"] ?? throw new InvalidOperationException("Missing: ConnectionString")));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown DbType from configuration '{Configuration["DbType"]}'");
            }

            switch (Configuration["MqType"])
            {
                case "rabbitMq":
                    services.Configure<RabbitMqConfig>(Configuration.GetSection("RabbitMq"));
                    services.AddTransient<IMqMessages, RabbitMqMessages>();
                    break;
                case "disabled":
                case "inMemory":
                    services.AddTransient<IMqMessages, MqMessagesNullObject>();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown MqType from configuration '{Configuration["MqType"]}'");
            }

            switch (Configuration["PdfStorageType"])
            {
                case "awsS3":
                    services.Configure<AwsS3Config>(Configuration.GetSection("AwsS3"));
                    services.AddSingleton<IStorage, AwsS3Storage>();
                    break;
                case "googleBucket":
                    services.Configure<GoogleCloudConfig>(Configuration.GetSection("GoogleCloud"));
                    services.AddTransient<IStorage, GoogleCloudPdfStorage>();
                    break;
                case "azureStorage":
                    services.Configure<AzureStorageConfig>(Configuration.GetSection("AzureStorage"));
                    services.AddSingleton<IStorage, AzureStorage>();
                    break;
                case "inMemory":
                    services.AddSingleton<IStorage, InMemoryPdfStorage>();
                    break;
                case "local":
                    services.Configure<LocalStorageConfig>(Configuration.GetSection("LocalStorage"));
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddSingleton<IStorage, LocalStorage>();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown PdfStorageType from configuration '{Configuration["PdfStorageType"]}'");
            }

            services.Configure<ApiKeyAuthenticationOptions>(Configuration.GetSection("ApiAuthentication"));

            services.Configure<HangfireConfig>(Configuration.GetSection("Hangfire"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UsePathBase(Configuration["PathBase"]);

            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pdf.Storage");
                c.RoutePrefix = "doc";

            });

            var workerCount = 4;
            if (!string.IsNullOrEmpty(Configuration["Hangfire:WorkerCount"]))
            {
                if (!int.TryParse(Configuration["Hangfire:WorkerCount"], out workerCount))
                {
                    throw new InvalidOperationException("Invalid WorkerCount on cofiguration.");
                }
            }

            var options = new BackgroundJobServerOptions
            {
                Queues = HangfireConstants.GetQueues().ToArray(),
                WorkerCount = workerCount,
            };

            app.UseEndpoints(endpoints =>
            {
                var config = app.ApplicationServices.GetRequiredService<IOptions<HangfireConfiguration>>();

                endpoints.MapHangfireDashboard("/hangfire", options: new DashboardOptions
                {
                    Authorization = [new HangfireBasicAuthenticationFilter(config)],
                });
            });

            switch (GetAppRole())
            {
                case "api":
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                    break;
                case "worker":
                    app.UseHangfireServer(options);
                    break;
                default:
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                    app.UseHangfireServer(options);
                    break;
            }
        }

        private string GetAppRole()
        {
            return Configuration["AppRole"] ?? "standalone";
        }
    }
}

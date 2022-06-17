using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pdf.Storage.Config;
using Pdf.Storage.Data;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Migrations;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Config;
using Pdf.Storage.Pdf.PdfStores;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;

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

            services
                .AddMvc(options => options.Filters.Add(new ValidateModelAttribute()))
                .AddNewtonsoftJson();

            services.AddSwaggerGenConfiguration();

            services.Configure<CommonConfig>(Configuration);

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

            app.Map("/hangfire", appBuilder =>
            {
                appBuilder.UseMiddleware<IpWhitelistMiddleware>();
                appBuilder.UseMiddleware<BasicAuthMiddleware>();
                appBuilder.UseHangfireDashboard("", new DashboardOptions
                {
                    Authorization = new IDashboardAuthorizationFilter[] {}
                });
            });

            switch (GetAppRole())
            {
                case "api":
                    app.UseEndpoints(endpoints => {
                        endpoints.MapControllers();
                    });
                    break;
                case "worker":
                    app.UseHangfireServer(options);
                    AddDeleteJob();
                    break;
                default:
                    app.UseEndpoints(endpoints => {
                        endpoints.MapControllers();
                    });
                    app.UseHangfireServer(options);
                    AddDeleteJob();
                    break;
            }
        }

        public void DeleteLocalStorage(string localStorageFolder)
        {
            var folder = new DirectoryInfo(localStorageFolder);
            var difference = new TimeSpan();
            foreach (var item in folder.EnumerateFiles())
            {
                difference = DateTime.Now - item.CreationTime;
                if(difference.TotalMinutes > 2)
                {
                    item.Delete();
                }
            }
        }

        private string GetAppRole()
        {
            return Configuration["AppRole"] ?? "standalone";
        }

        private void AddDeleteJob()
        {
            if (!string.IsNullOrEmpty(Configuration["LocalStorage:Folder"]) && Configuration["PdfStorageType"] == "local")
            {
                var pattern = @"^[\/][a-zA-Z0-9\/_-]+$";
                var localStorageFolder = Configuration["LocalStorage:Folder"];
                if (Regex.IsMatch(localStorageFolder.Trim(), pattern))
                {
                    RecurringJob.AddOrUpdate("emptyLocalStorageFolder", () => DeleteLocalStorage(localStorageFolder), Configuration["TimeToEmptyStorageFolder:Crontab"]);
                }
                else
                {
                    throw new InvalidOperationException("Invalid local storage folder on configuration.");
                }
            }
        }
    }
}

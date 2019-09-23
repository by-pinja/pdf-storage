using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;
using PuppeteerSharp;

namespace Pdf.Storage.Migrations
{
    public static class HostStartupExtensions
    {
        public static IWebHost MigrateDb(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<PdfDataContext>();

                    if (context.Database.IsInMemory())
                    {
                        logger.LogWarning("In memory database is used, skipping migrations.");
                    }
                    else if (context.Database.IsSqlServer())
                    {
                        services.GetRequiredService<MsSqlDataContextForMigrations>()
                            .Database.Migrate();
                    }
                    else if (context.Database.IsNpgsql())
                    {
                        services.GetRequiredService<NpSqlDataContextForMigrations>()
                            .Database.Migrate();
                    }
                    else
                    {
                        throw new InvalidOperationException("No known migration route.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    throw;
                }
            }

            return host;
        }

        public static async Task<IWebHost> DownloadPrequisitiesIfNeeded(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                // if(Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") == "1")
                // {
                //     logger.LogInformation("Running in container (environment RUNNING_IN_CONTAINER=1), skipping all pre-install steps.");
                //     return host;
                // }

                logger.LogInformation("Making sure correct chromium for puppeteer is available.");
                //await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            }

            return host;
        }
    }
}
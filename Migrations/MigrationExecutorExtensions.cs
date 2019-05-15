using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;

namespace Pdf.Storage.Migrations
{
    public static class MigrationExecutorExtensions
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
    }
}
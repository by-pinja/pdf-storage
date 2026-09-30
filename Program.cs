using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Pdf.Storage.Migrations;

namespace Pdf.Storage
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = BuildHost(args);

            await host.DownloadPrerequisitesIfNeeded();

            host.MigrateDb();

            await host.RunAsync();
        }

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build();
    }
}

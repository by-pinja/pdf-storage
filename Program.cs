using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Pdf.Storage.Migrations;

namespace Pdf.Storage
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var isService = WindowsServiceHelpers.IsWindowsService();

            var host = BuildWebHost(args, isService).Build();
            await host.DownloadPrequisitiesIfNeeded();

            host.MigrateDb();

            await host.RunAsync();
        }

        public static IHostBuilder BuildWebHost(string[] args, bool isService)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseContentRoot(Directory.GetCurrentDirectory());

            if (isService)
            {
                using var process = Process.GetCurrentProcess();
                var pathToExe = process.MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                builder.UseContentRoot(pathToContentRoot);
            }
            else
            {
                builder.UseConsoleLifetime();
            }

            return builder;
        }
    }
}

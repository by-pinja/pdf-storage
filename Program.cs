﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Pdf.Storage.Migrations;

namespace Pdf.Storage
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = BuildWebHost(args);

            await host.DownloadPrequisitiesIfNeeded();

            host.MigrateDb();

            await host.RunAsync();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();
    }
}

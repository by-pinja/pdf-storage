using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;
using Pdf.Storage.Migrations;

namespace Pdf.Storage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.MigrateDb();
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://0.0.0.0:5000")
                .Build();
    }
}

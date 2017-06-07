using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Pdf.Storage.Data;
using Pdf.Storage.Pdf;
using Pdf.Storage.Test;
using Pdf.Storage.Util;
using Swashbuckle.AspNetCore.Swagger;

namespace Pdf.Storage
{
    public class Startup
    {
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
                        Title = "Pdf service",
                        Version = "v1",
                        Description = File.ReadAllText(Path.Combine(basePath, "ApiDescription.md"))
                    });
            });

            services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase());

            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddSingleton<IPdfStorage, InMemoryPdfStorage>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authorization Service");
                c.RoutePrefix = "doc";
            });

            app.UseMvc();
        }
    }
}

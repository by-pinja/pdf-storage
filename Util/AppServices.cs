using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Swashbuckle.AspNetCore.Filters;

namespace Pdf.Storage.Util
{
    public static class AppServices
    {
        public static IServiceCollection AddSwaggerGenConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerExamplesFromAssemblyOf<Startup>();

            return services.AddSwaggerGen(c =>
            {
                var basePath = AppContext.BaseDirectory;

                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Pdf.Storage",
                        Version = "v1",
                        Description = File.ReadAllText(Path.Combine(basePath, "ApiDescription.md"))
                    });

                c.ExampleFilters();

                c.AddSecurityDefinition("ApiKey", ApiKey.OpenApiSecurityScheme);
                c.AddSecurityRequirement(ApiKey.OpenApiSecurityRequirement("ApiKey"));

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(basePath, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public static IServiceCollection AddCommonAppServices(this IServiceCollection services)
        {
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();
            services.AddSingleton<Uris>();
            services.AddSingleton<TemplatingEngine>();
            services.AddTransient<IPdfMerger, PdfMerger>();

            return services;
        }
    }
}
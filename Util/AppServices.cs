using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
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

                // Protacon.NetCore.WebApi.ApiKeyAuth ships these as Microsoft.OpenApi v1 types,
                // which Swashbuckle 10 (Microsoft.OpenApi v2) can't use, so they're defined here.
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Apikey authorization. Example: \"Authorization: ApiKey {key}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
                });

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
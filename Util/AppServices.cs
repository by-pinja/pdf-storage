using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
using Protacon.NetCore.WebApi.ApiKeyAuth;

namespace Pdf.Storage.Util
{
    public static class AppServices
    {
        public static IServiceCollection AddSwaggerGenConfiguration(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                var basePath = System.AppContext.BaseDirectory;

                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Pdf.Storage",
                        Version = "v1",
                        Description = File.ReadAllText(Path.Combine(basePath, "ApiDescription.md"))
                    });

                c.AddSecurityDefinition("ApiKey", ApiKey.OpenApiSecurityScheme);
                c.AddSecurityRequirement(ApiKey.OpenApiSecurityRequirement("ApiKey"));
            });
        }

        public static IServiceCollection AddCommonAppServices(this IServiceCollection services)
        {
            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();
            services.AddSingleton<Uris>();
            services.AddSingleton<TemplatingEngine>();
            services.AddTransient<IPdfMerger, PdfMerger>();

            return services;
        }
    }
}
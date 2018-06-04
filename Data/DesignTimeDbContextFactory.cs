using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Pdf.Storage.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PdfDataContext>
    {
        PdfDataContext IDesignTimeDbContextFactory<PdfDataContext>.CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PdfDataContext>();

            builder.UseNpgsql("User ID=postgres;Password=passwordfortesting;Host=localhost;Port=5432;Database=pdfstorage;Pooling=true;");

            return new PdfDataContext(builder.Options);
        }
    }
}
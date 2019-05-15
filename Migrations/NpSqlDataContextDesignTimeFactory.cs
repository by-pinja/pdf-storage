using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pdf.Storage.Data;

namespace Pdf.Storage.Migrations
{
    public class NpSqlDataContextDesignTimeFactory : IDesignTimeDbContextFactory<NpSqlDataContextForMigrations>
    {
        NpSqlDataContextForMigrations IDesignTimeDbContextFactory<NpSqlDataContextForMigrations>.CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PdfDataContext>();
            return new NpSqlDataContextForMigrations(builder.Options);
        }
    }
}
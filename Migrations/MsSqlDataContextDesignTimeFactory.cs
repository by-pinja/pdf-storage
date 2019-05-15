using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pdf.Storage.Data;

namespace Pdf.Storage.Migrations
{
    public class MsSqlDataContextDesignTimeFactory : IDesignTimeDbContextFactory<MsSqlDataContextForMigrations>
    {
        MsSqlDataContextForMigrations IDesignTimeDbContextFactory<MsSqlDataContextForMigrations>.CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PdfDataContext>();
            return new MsSqlDataContextForMigrations(builder.Options);
        }
    }
}
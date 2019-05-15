using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pdf.Storage.Data;

namespace Pdf.Storage.Migrations
{
    public class NpSqlDataContextForMigrations : PdfDataContext
    {
        public NpSqlDataContextForMigrations(DbContextOptions<PdfDataContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(!options.IsConfigured)
                options.UseNpgsql("User ID=postgres;Password=testpassword;Host=localhost;Port=5432;Database=pdf-storage;Pooling=true;");
        }
    }
}
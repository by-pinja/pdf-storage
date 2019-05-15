using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pdf.Storage.Data;

namespace Pdf.Storage.Migrations
{

    public class MsSqlDataContextForMigrations: PdfDataContext
    {
        public MsSqlDataContextForMigrations(DbContextOptions<PdfDataContext> options) : base(options) {}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(!options.IsConfigured)
                options.UseSqlServer("Server=localhost,1433;Database=pdf-storage;User=sa;Password=testpassword1#!;");
        }
    }
}
using Microsoft.EntityFrameworkCore;

namespace Pdf.Storage.Data
{
    public class PdfDataContext : DbContext
    {
        public PdfDataContext(DbContextOptions<PdfDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PdfOpenedEntity>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Usage);
        }

        public DbSet<PdfEntity> PdfFiles { get; set; }
        public DbSet<PdfRawData> RawData { get; set; }
    }
}

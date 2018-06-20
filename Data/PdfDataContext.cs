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

            modelBuilder.Entity<PdfEntity>()
                .HasIndex(b => b.FileId);

            modelBuilder.Entity<PdfEntity>()
                .HasIndex(b => b.GroupId);
        }

        public DbSet<PdfEntity> PdfFiles { get; set; }
        public DbSet<PdfRawDataEntity> RawData { get; set; }
    }
}

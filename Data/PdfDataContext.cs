using Microsoft.EntityFrameworkCore;

namespace Pdf.Storage.Data
{
    public class PdfDataContext : DbContext
    {
        public PdfDataContext(DbContextOptions<PdfDataContext> options) : base(options)
        {
        }

        public DbSet<PdfEntity> PdfFiles { get; set; }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Pdf.Storage.Data;

namespace Pdf.Storage.Migrations
{
    [DbContext(typeof(PdfDataContext))]
    partial class PdfDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Pdf.Storage.Data.PdfEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileId");

                    b.Property<string>("GroupId");

                    b.Property<int>("OpenedTimes");

                    b.Property<bool>("Processed");

                    b.HasKey("Id");

                    b.ToTable("PdfFiles");
                });
        }
    }
}

﻿using System;
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

            modelBuilder.Entity("Pdf.Storage.Data.PdfOpenedEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ParentId");

                    b.Property<DateTime>("Stamp");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("PdfOpenedEntity");
                });

            modelBuilder.Entity("Pdf.Storage.Data.PdfOpenedEntity", b =>
                {
                    b.HasOne("Pdf.Storage.Data.PdfEntity", "Parent")
                        .WithMany("Usage")
                        .HasForeignKey("ParentId");
                });
        }
    }
}

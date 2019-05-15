using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Pdf.Storage.Migrations
{
    public partial class PdfQueuePriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "PdfFiles",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RawData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Html = table.Column<string>(nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    ParentId = table.Column<Guid>(nullable: false),
                    TemplateData = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawData", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawData");

            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "PdfFiles");
        }
    }
}

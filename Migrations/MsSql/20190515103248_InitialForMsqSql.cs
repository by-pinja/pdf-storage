using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pdf.Storage.Migrations.MsSql
{
    public partial class InitialForMsqSql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PdfFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<string>(nullable: true),
                    FileId = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Processed = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    OpenedTimes = table.Column<int>(nullable: false),
                    HangfireJobId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: false),
                    Html = table.Column<string>(nullable: true),
                    TemplateData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PdfOpenedEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Stamp = table.Column<DateTime>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfOpenedEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdfOpenedEntity_PdfFiles_ParentId",
                        column: x => x.ParentId,
                        principalTable: "PdfFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PdfFiles_FileId",
                table: "PdfFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_PdfFiles_GroupId",
                table: "PdfFiles",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PdfOpenedEntity_ParentId",
                table: "PdfOpenedEntity",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PdfOpenedEntity");

            migrationBuilder.DropTable(
                name: "RawData");

            migrationBuilder.DropTable(
                name: "PdfFiles");
        }
    }
}

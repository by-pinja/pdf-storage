using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pdf.Storage.Migrations
{
    public partial class PdfUsage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PdfOpenedEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: true),
                    Stamp = table.Column<DateTime>(nullable: false)
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
                name: "IX_PdfOpenedEntity_ParentId",
                table: "PdfOpenedEntity",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PdfOpenedEntity");
        }
    }
}

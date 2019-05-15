using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Pdf.Storage.Migrations
{
    public partial class PdfFileIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PdfFiles_FileId",
                table: "PdfFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_PdfFiles_GroupId",
                table: "PdfFiles",
                column: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PdfFiles_FileId",
                table: "PdfFiles");

            migrationBuilder.DropIndex(
                name: "IX_PdfFiles_GroupId",
                table: "PdfFiles");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Pdf.Storage.Migrations
{
    public partial class DeleteApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfOpenedCallbackUri",
                table: "PdfFiles");

            migrationBuilder.AddColumn<bool>(
                name: "Removed",
                table: "PdfFiles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Removed",
                table: "PdfFiles");

            migrationBuilder.AddColumn<string>(
                name: "PdfOpenedCallbackUri",
                table: "PdfFiles",
                nullable: true);
        }
    }
}

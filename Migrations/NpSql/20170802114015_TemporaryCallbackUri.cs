using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pdf.Storage.Migrations
{
    public partial class TemporaryCallbackUri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfOpenedCallbackUri",
                table: "PdfFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfOpenedCallbackUri",
                table: "PdfFiles");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pdf.Storage.Migrations.NpSql
{
    /// <inheritdoc />
    public partial class UseTimestampWithTimeZone : Migration
    {
        // Npgsql 6+ maps DateTime to 'timestamp with time zone'. Older migrations don't pin the column
        // type, so databases created before Npgsql 6 have 'timestamp without time zone' while newer ones
        // already have 'timestamp with time zone'. Convert only the former. Values have always been
        // written with DateTime.UtcNow, so they are interpreted as UTC regardless of the session time zone.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlterColumnIfType("PdfOpenedEntity", "Stamp", "timestamp without time zone", "timestamp with time zone"));
            migrationBuilder.Sql(AlterColumnIfType("PdfFiles", "Created", "timestamp without time zone", "timestamp with time zone"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlterColumnIfType("PdfOpenedEntity", "Stamp", "timestamp with time zone", "timestamp without time zone"));
            migrationBuilder.Sql(AlterColumnIfType("PdfFiles", "Created", "timestamp with time zone", "timestamp without time zone"));
        }

        private static string AlterColumnIfType(string table, string column, string fromType, string toType) => $@"
DO $$
BEGIN
    IF (SELECT data_type FROM information_schema.columns
        WHERE table_schema = current_schema() AND table_name = '{table}' AND column_name = '{column}') = '{fromType}' THEN
        ALTER TABLE ""{table}"" ALTER COLUMN ""{column}"" TYPE {toType} USING ""{column}"" AT TIME ZONE 'UTC';
    END IF;
END $$;";
    }
}

# Supports multiple database types, for this reason preferred way to create migrations for all
# at once is this script.
[CmdLetBinding()]
Param(
    [Parameter(Mandatory)]
    [string]
    $MigrationName,

    [ValidateSet("Add", "Remove")]
    [string]
    $Operation = "Add"
)

if($Operation -eq "Remove")
{
    dotnet ef migrations remove --context "NpSqlDataContext"
    dotnet ef migrations remove --context "MsSqlDataContext"
}
else
{
    if(-not $MigrationName)
    {
        throw "MigrationName parameter is required when adding new migrations."
    }

    #dotnet ef migrations add --context "NpSqlDataContextForMigrations" $MigrationName -o ./Migrations/NpSql/
    dotnet ef migrations add --context "MsSqlDataContextForMigrations" $MigrationName -o ./Migrations/MsSql/
}
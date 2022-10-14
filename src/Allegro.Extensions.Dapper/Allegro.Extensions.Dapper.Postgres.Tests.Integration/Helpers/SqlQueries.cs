namespace Allegro.Extensions.Dapper.Postgres.Tests.Integration.Helpers;

public static class SqlQueries
{
    public const string TableName = "TestTable";

    public static string CreateTableIfNotExists =>
        @"
            CREATE TABLE IF NOT EXISTS TestTable
            (
                Id              SERIAL primary key,
                Username        VARCHAR(40) UNIQUE
            );
        ";

    public static string InsertRowWithReturning =>
        $@"
            INSERT INTO {TableName}
            (
                Username
            )
            VALUES
            (
                @Username
            )
            RETURNING
                id AS Id,
                username AS Username;
        ";

    public static string DropTableIfExists =>
        $@"DROP TABLE IF EXISTS {TableName}";

    public static string SelectAllFromTable =>
        $@"SELECT
            Id,
            Username
        FROM {TableName}";

    public static string SelectFirstFromTable =>
        SelectAllFromTable + " LIMIT 1";
}
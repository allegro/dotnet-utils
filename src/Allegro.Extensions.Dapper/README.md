# Allegro.Extensions.Dapper

This library contains useful utilities for simpler usage of Dapper library.

In this library you have generic implementation of DatabaseClient which is database agnostic.

You can use already created utilities for specific databases.
- Allegro.Extensions.Dapper.Postgres

Then you will be able to use IDatabaseClient and perform SQL operations.

####If you want to use this library on it's own, please remember to implement IDatabaseConnectionFactory. Example implementation (for Postgres):
```c#
public class PostgresDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly DatabaseConfiguration _databaseConfiguration;

    public PostgresDatabaseConnectionFactory(
        DatabaseConfiguration databaseConfiguration)
    {
        _databaseConfiguration = databaseConfiguration;
    }

    public DbConnection Create() =>
        new NpgsqlConnection(_databaseConfiguration.ConnectionString);
}
```

Configure this feature in `Startup.cs`:
```c#
services
    .AddDapperClient()
    .AddSingleton(new DatabaseConfiguration
    {
        ConnectionString = connectionString
    })
    .AddSingleton<IDatabaseConnectionFactory, PostgresDatabaseConnectionFactory>();
```

# Allegro.Extensions.Dapper.Postgres

This library contains useful utilities for simpler usage for Postgres database with Dapper library.

**Remember** to configure this feature in `Startup.cs`:

```c#
services
    .AddDapperClient()
    .AddDapperPostgres(connectionString);
```

Then you will be able to use IDapperPostgresBinaryCopyClient and IDapperClient services and perform operations SQL queries against database.

Example usages:
```c#
public const string TableName = "TestTable";

//create table
await _dapperClient.ExecuteAsync($@"CREATE TABLE IF NOT EXISTS {TableName}
            (
                Id              SERIAL primary key,
                Username        VARCHAR(40) UNIQUE
            );
        ");

//drop table
await _dapperClient.ExecuteAsync($@"DROP TABLE IF EXISTS {TableName}");

//insert data into table with returning in transaction
await _dapperClient.ExecuteAndQueryInTransactionAsync<TestTable>($@"
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
        ");

//query records
await _dapperClient.QueryAsync<TestTable>($@"SELECT
            Id,
            Username
        FROM {TableName});
```
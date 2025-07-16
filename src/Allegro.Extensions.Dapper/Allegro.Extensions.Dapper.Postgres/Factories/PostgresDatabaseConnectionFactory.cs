using System.Data.Common;
using Allegro.Extensions.Dapper.Abstractions;
using Allegro.Extensions.Dapper.Configurations;
using Npgsql;

namespace Allegro.Extensions.Dapper.Postgres.Factories;

/// <summary>
/// Database connection factory for Postgres database.
/// </summary>
internal sealed class PostgresDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly DatabaseConfiguration _databaseConfiguration;

    public PostgresDatabaseConnectionFactory(
        DatabaseConfiguration databaseConfiguration)
    {
        _databaseConfiguration = databaseConfiguration;
    }

    /// <summary>
    /// Initialize new NpgsqlConnection with registered as configuration connection string.
    /// </summary>
    public DbConnection Create() =>
        new NpgsqlConnection(_databaseConfiguration.ConnectionString);
}
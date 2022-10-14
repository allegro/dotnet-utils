using System.Data.Common;

namespace Allegro.Extensions.Dapper.Abstractions;

/// <summary>
/// Factory to create specific DbConnection (e. g. Postgres, MySQL DbConnection).
/// </summary>
public interface IDatabaseConnectionFactory
{
    /// <summary>
    /// Initialize database connection with registered as configuration connection string.
    /// </summary>
    DbConnection Create();
}
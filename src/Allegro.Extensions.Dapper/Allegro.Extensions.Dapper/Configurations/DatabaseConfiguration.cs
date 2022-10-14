namespace Allegro.Extensions.Dapper.Configurations;

/// <summary>
/// Database configuration options.
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Connection string to a database.
    /// </summary>
    public string ConnectionString { get; init; } = null!;
}
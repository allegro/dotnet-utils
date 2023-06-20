using Allegro.Extensions.Dapper.Abstractions;
using Allegro.Extensions.Dapper.Configurations;
using Allegro.Extensions.Dapper.Postgres.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Dapper.Postgres.Extensions;

/// <summary>
/// Service Collection Extensions to register dapper postgress services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Dapper Postgres utilities with passed connection string as a parameter.
    /// </summary>
    /// <param name="services">Target of an extension method</param>
    /// <param name="connectionString">Postgres database connection string</param>
    public static IServiceCollection AddDapperPostgres(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "Empty connection string");
        }

        return services
            .AddSingleton(new DatabaseConfiguration
            {
                ConnectionString = connectionString
            })
            .AddSingleton<PostgresDatabaseConnectionFactory>()
            .AddSingleton<IDatabaseConnectionFactory, PostgresDatabaseConnectionFactory>()
            .AddSingleton<IDapperPostgresBinaryCopyClient, DapperPostgresBinaryCopyClient>();
    }
}
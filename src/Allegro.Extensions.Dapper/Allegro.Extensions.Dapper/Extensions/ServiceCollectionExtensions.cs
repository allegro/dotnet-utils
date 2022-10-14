using Allegro.Extensions.Dapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Dapper.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Dapper Client.
    /// </summary>
    /// <param name="services">Target of an extension method</param>
    public static IServiceCollection AddDapperClient(
        this IServiceCollection services)
    {
        return services
            .AddSingleton<IDapperClient, DapperClient>();
    }
}
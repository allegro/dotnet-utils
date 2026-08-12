using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Allegro.Extensions.Configuration.Client;

public static class StartupExtensions
{
    public static IServiceCollection AddConfeatureServiceClient(
        this IServiceCollection services,
        Func<IServiceProvider, HttpClient> httpClientFactory)
    {
        return services.AddSingleton(
            sp =>
            {
                var httpClient = httpClientFactory(sp);
                return new ConfeatureServiceClient(httpClient);
            });
    }
}
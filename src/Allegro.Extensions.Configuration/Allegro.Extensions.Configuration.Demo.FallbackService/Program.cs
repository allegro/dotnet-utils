using Allegro.Extensions.Configuration.Api;
using Allegro.Extensions.Configuration.Api.Services;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.Configuration.Demo.FallbackService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Basic Confeature settings
        var confeatureOptions = new ConfeatureOptions
        {
            IsEnabled = true,
            ServiceName = "demo",
            AuthorizationPolicy = null,
        };

        // Add the global configuration source
        builder.Configuration.AddGlobalConfiguration(
            confeatureOptions,
            builder.Environment);

        // Register the Confeature
        builder.Services.AddConfeatureFallbackService(confeatureOptions);

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ISecretsProvider).Assembly);
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
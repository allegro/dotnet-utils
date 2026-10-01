using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Demo.Configuration;
using Allegro.Extensions.Configuration.Extensions;
using Allegro.Extensions.Configuration.Wrappers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.Configuration.Demo;

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

        // This is just an example of marking configuration source as sensitive.
        // In real-world scenario this would be something like a K8S Secret mounted on volume.
        // Values from this source will not be visible in /configuration endpoint response.
        builder.Configuration.WrapSensitive().AddJsonFile("appsettings.Secret.json");

        // Add the global configuration source
        builder.Configuration.AddGlobalConfiguration(
            confeatureOptions,
            builder.Environment);

        // Register the Confeature and all configuration classes (available later using IOptions<> pattern)
        builder.Services
            .AddConfeature(confeatureOptions)
            .RegisterConfig<TestLocalConfig>(builder.Configuration, "TestConfig")
            .RegisterGlobalConfig<TestGlobalConfig>(builder.Configuration, confeatureOptions);

        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
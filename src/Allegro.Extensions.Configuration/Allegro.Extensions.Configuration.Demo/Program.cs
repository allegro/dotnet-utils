using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Demo.Configuration;
using Allegro.Extensions.Configuration.Extensions;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.Configuration.Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var confeatureOptions = new ConfeatureOptions
        {
            IsEnabled = true,
            ServiceName = "demo",
            AuthorizationPolicy = null,
        };

        builder.Configuration.AddGlobalConfiguration(
            confeatureOptions,
            builder.Environment);

        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddConfeature(confeatureOptions)
            .RegisterGlobalConfig<TestGlobalConfig>(builder.Configuration, confeatureOptions);

        var app = builder.Build();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
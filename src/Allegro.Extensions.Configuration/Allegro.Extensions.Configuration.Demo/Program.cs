using Allegro.Extensions.Configuration.Extensions;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.Configuration.Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddConfeature();

        var app = builder.Build();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
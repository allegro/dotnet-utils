using Allegro.Extensions.Cqrs.Commands;
using Allegro.Extensions.Cqrs.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Demo;

public class Startup
{
    private readonly IWebHostEnvironment _env;

    public Startup(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void Configure(IApplicationBuilder app)
    {
        app
            .UseRouting()
            .UseSwagger()
            .UseSwaggerUI()
            .UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var cqrsAssemblies = new[] { typeof(Startup).Assembly };
        services
            .AddCommands(cqrsAssemblies)
            .AddQueries(cqrsAssemblies)
            .AddControllers();

        services.AddSwaggerGen();
    }
}
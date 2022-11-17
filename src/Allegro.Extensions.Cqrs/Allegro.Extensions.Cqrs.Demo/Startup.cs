using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Allegro.Extensions.Cqrs.Commands;
using Allegro.Extensions.Cqrs.Demo.Commands;
using Allegro.Extensions.Cqrs.Demo.Queries;
using Allegro.Extensions.Cqrs.FluentValidations;
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
            .AddCqrsFluentValidations(cqrsAssemblies)
            .AddControllers();
        services.TryDecorate<ICommandHandler<BarCommand>, BarCommandHandlerDecorator>();
        services.TryDecorate<IQueryHandler<BarQuery, BarData>, BarQueryHandlerDecorator>();
        services.AddSwaggerGen();
    }
}
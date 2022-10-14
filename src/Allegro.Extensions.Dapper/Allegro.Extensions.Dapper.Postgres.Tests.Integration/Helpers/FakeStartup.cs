using Allegro.Extensions.Dapper.Extensions;
using Allegro.Extensions.Dapper.Postgres.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Dapper.Postgres.Tests.Integration.Helpers;

public class FakeStartup
{
    private readonly IConfiguration _configuration;

    public FakeStartup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration["PostgresSDK:ConnectionString"];

        services
            .AddDapperClient()
            .AddDapperPostgres(connectionString);
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}
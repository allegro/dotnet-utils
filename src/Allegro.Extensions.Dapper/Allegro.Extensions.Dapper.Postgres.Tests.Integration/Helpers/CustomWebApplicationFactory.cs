using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Allegro.Extensions.Dapper.Postgres.Tests.Integration.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<FakeStartup>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return base.CreateHostBuilder()!
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Test.json");
            })
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .UseStartup<FakeStartup>();
                })
            .UseEnvironment(Environments.Development);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        return base.CreateHost(builder);
    }
}
using Allegro.Extensions.Configuration.Demo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vabank.Confeature.Integration.Tests;

namespace Allegro.Extensions.Configuration.Tests.Integration.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string AspNetEnvironment { get; set; } = Environments.Development;

    /// <remarks>
    /// If error on dispose please relate to https://github.com/djluck/prometheus-net.DotNetRuntime/issues/65
    /// and catch it in Dispose as a workaround
    /// </remarks>
    protected override IHostBuilder CreateHostBuilder()
    {
        Environment.SetEnvironmentVariable("IntegrationTesting", "true");
        // we want to control the environment with UseEnvironment instead of env var
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "");

        return base.CreateHostBuilder()
            .ConfigureServices(
                services =>
                {
                    services.AddSingleton<TestService>();
                })
            .ConfigureWebHostDefaults(
                webBuilder =>
                    webBuilder.UseEnvironment(AspNetEnvironment)
            );
    }
}
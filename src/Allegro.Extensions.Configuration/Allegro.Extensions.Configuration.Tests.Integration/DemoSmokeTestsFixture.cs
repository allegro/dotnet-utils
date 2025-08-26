using Allegro.Extensions.Configuration.Demo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Vabank.Confeature.Tests;

namespace Vabank.Confeature.Integration.Tests;

public class DemoSmokeTestsFixture : ConfeatureSmokeTest<Program>
{
    public DemoSmokeTestsFixture(WebApplicationFactory<Program> factory)
        : base(
            factory.WithWebHostBuilder(
                cfg => cfg.ConfigureServices(sc => sc.Configure<TestConfig>(o => o.SomeValue = 1))))
    {
    }
}
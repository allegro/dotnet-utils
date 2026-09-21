using Allegro.Extensions.Dapper.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Allegro.Extensions.Dapper.Postgres.Tests.Integration;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void BinaryCopyClient_ShouldUseRegisteredConnectionFactory()
    {
        var services = new ServiceCollection()
            .AddDapperPostgres("Host=localhost;Database=test;Username=test");

        var concreteFactoryRegistration = services.Single(
            descriptor => descriptor.ServiceType.Name == "PostgresDatabaseConnectionFactory");
        services.Remove(concreteFactoryRegistration);
        services.RemoveAll<IDatabaseConnectionFactory>();
        services.AddSingleton(Mock.Of<IDatabaseConnectionFactory>());

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider
            .GetRequiredService<IDapperPostgresBinaryCopyClient>()
            .Should()
            .NotBeNull();
    }
}
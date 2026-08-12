using System.Net;
using System.Net.Http.Json;
using Allegro.Extensions.Configuration.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Vabank.Confeature.Tests;

// ReSharper disable MemberCanBePrivate.Global

/// <summary>
/// Derive from this class to include a simple smoke test for the confeature integration.
/// </summary>
/// <typeparam name="T">Entrypoint of your ASP.NET Core application, most likely the Program class.</typeparam>
public abstract class ConfeatureSmokeTest<T> : IClassFixture<WebApplicationFactory<T>>
    where T : class
{
    protected readonly WebApplicationFactory<T> Factory;

    protected ConfeatureSmokeTest(WebApplicationFactory<T> factory) => Factory = factory;

    [Fact]
    public async Task Should_GetNonEmptyResponse_WhenQueryingConfigurationEndpoint()
    {
        // Arrange
        // for integration tests running on kubernetes-hosted build agents
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", "");
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/configuration");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ConfigurationResponse>();
        content.Should().NotBeNull();
        content?.Configuration.Should().NotBeNullOrEmpty();
        content?.Providers.Should().NotBeNullOrEmpty();
    }
}
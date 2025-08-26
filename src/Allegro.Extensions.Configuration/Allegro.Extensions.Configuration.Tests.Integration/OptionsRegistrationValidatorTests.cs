using Allegro.Extensions.Configuration;
using Allegro.Extensions.Configuration.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Vabank.Confeature.Integration.Tests;

[Collection(CustomWebApplicationFactoryCollection.Name)]
public class OptionsRegistrationValidatorTests
{
    private const string ShouldSkipTests = "Integration tests skipped as they're not working on the build agents";

    private readonly CustomWebApplicationFactory _factory;

    // Set this to null in order to enable integration tests

    public OptionsRegistrationValidatorTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact(Skip = ShouldSkipTests)]
    public void ShouldThrowAnException_WhenConfigurationDtoIsNotRegistered()
    {
        // Arrange
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", "");
        var func = () => _factory.CreateClient();

        // Act & Assert
        func
            .Should()
            .Throw<Exception>(because: "Configuration DTO is not registered properly")
            .WithMessage($"*{nameof(TestService)}*{nameof(TestConfig)}*DisableOptionsRegistrationValidation*");
    }

    [Fact(Skip = ShouldSkipTests)]
    public void ShouldSkipValidation_WhenEnvironmentVariableIsSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", "");
        Environment.SetEnvironmentVariable("DisableOptionsRegistrationValidation", bool.TrueString);
        var func = () => _factory.CreateClient();

        // Act & Assert
        func
            .Should()
            .NotThrow(because: "Validation is turned off using the feature flag");
    }

    [Fact(Skip = ShouldSkipTests)]
    public void ShouldSkipGenericTypes()
    {
        // Arrange
        // We're adding the TestConfig options registration to prevent them from causing an exception
        // We only want to check the generic IOptions<T> validation behaviour
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", "");
        var func = () => _factory
            .WithWebHostBuilder(cfg => cfg.ConfigureServices(sc => sc.Configure<TestConfig>(o => o.SomeValue = 1)))
            .CreateClient();

        // Act & Assert
        func
            .Should()
            .NotThrow(because: "Generic types should be skipped");
    }
}

public class TestService
{
    public TestService(IOptions<TestConfig> testConfig) { }
}

public class TestConfig : IConfigurationMarker
{
    public int SomeValue { get; set; }
}

public class GenericTestService<T> where T : TestConfig
{
    public GenericTestService(IOptions<T> testConfig) { }
}
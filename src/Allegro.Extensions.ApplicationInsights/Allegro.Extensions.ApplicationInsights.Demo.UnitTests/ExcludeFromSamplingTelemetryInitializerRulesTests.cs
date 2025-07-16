using Allegro.Extensions.ApplicationInsights.AspNetCore;
using AutoFixture;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Moq;

namespace Allegro.Extensions.ApplicationInsights.Demo.UnitTests;

internal sealed class
    CustomExcludeFromSamplingTelemetryInitializer : ExcludeFromSamplingTelemetryInitializer<CustomDependencyForFilter,
        RequestForFilter>
{
    public CustomExcludeFromSamplingTelemetryInitializer(
        IOptions<ExcludeFromSamplingTelemetryConfig> config,
        ITelemetryInitializerLogger logger,
        Func<DependencyTelemetry, CustomDependencyForFilter> dependencyMap,
        Func<RequestTelemetry, RequestForFilter> requestMap) : base(config, logger, dependencyMap, requestMap)
    {
    }
}

public class ExcludeFromSamplingTelemetryInitializerRulesTests
{
    private static readonly IOptions<ExcludeFromSamplingTelemetryConfig> Default = Options.Create(
        new ExcludeFromSamplingTelemetryConfig()
        {
            DependencyRules =
                new List<string>()
                {
                    "Type eq 'Azure Service Bus' and (duration ge 5000 or success eq false)", "Team eq 'skyfall'"
                }.ToDictionary(_ => Guid.NewGuid().ToString(), p => p),
            RequestRules =
                new List<string>() { "CloudRoleName eq 'MyService' and Name eq 'POST MyRoute/MyEndpoint'" }
                    .ToDictionary(_ => Guid.NewGuid().ToString(), p => p)
        });

    [Theory]
    [InlineData("HTTP", 100, "any team", false, false)]
    [InlineData("Azure Service Bus", 100, "any team", true, false)]
    [InlineData("Azure Service Bus", 100, "any team", false, true)]
    [InlineData("Azure Service Bus", 5001, "any team", true, true)]
    [InlineData("HTTP", 100, "skyfall", false, true)]
    public void Dependencies_TestDefaultSettings(
        string type,
        int duration,
        string team,
        bool success,
        bool shouldExclude)
    {
        var mockLogger = new Mock<ITelemetryInitializerLogger>();
        var fixture = new Fixture();
        var dependency = new DependencyTelemetry(type, fixture.Create<string>(), fixture.Create<string>(), null)
        {
            Duration = new TimeSpan(0, 0, 0, 0, duration)
        };
        dependency.Context.Cloud.RoleName = fixture.Create<string>();
        dependency.ResultCode = fixture.Create<string>();
        dependency.Success = success;
        dependency.Properties[nameof(CustomTelemetryCloudApplicationInfo.TeamName)] = team;

        CustomDependencyForFilter DependencyMap(DependencyTelemetry d) => new(d);

        RequestForFilter RequestMap(RequestTelemetry r) => new(r);

        var telemetryInitializer =
            new CustomExcludeFromSamplingTelemetryInitializer(Default, mockLogger.Object, DependencyMap, RequestMap);

        telemetryInitializer.Initialize(dependency);

        ((ISupportSampling)dependency).SamplingPercentage.Should().Be(shouldExclude ? 100 : null);
        mockLogger.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("MyService", "POST MyRoute/MyEndpoint", true)]
    [InlineData("MyService", "POST MyRoute/OtherEndpoint", false)]
    public void Requests_TestDefaultSettings(string cloudRoleName, string name, bool shouldExclude)
    {
        var mockLogger = new Mock<ITelemetryInitializerLogger>();
        var fixture = new Fixture();
        var requestTelemetry = new RequestTelemetry(
            name,
            DateTimeOffset.UtcNow,
            new TimeSpan(0, 0, 0, 0, fixture.Create<int>()),
            fixture.Create<string>(),
            fixture.Create<bool>());
        requestTelemetry.Context.Cloud.RoleName = cloudRoleName;

        CustomDependencyForFilter DependencyMap(DependencyTelemetry d) => new(d);

        RequestForFilter RequestMap(RequestTelemetry r) => new(r);

        var telemetryInitializer =
            new CustomExcludeFromSamplingTelemetryInitializer(Default, mockLogger.Object, DependencyMap, RequestMap);

        telemetryInitializer.Initialize(requestTelemetry);

        ((ISupportSampling)requestTelemetry).SamplingPercentage.Should().Be(shouldExclude ? 100 : null);
        mockLogger.VerifyNoOtherCalls();
    }
}
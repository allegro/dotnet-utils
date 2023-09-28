using Allegro.Extensions.ApplicationInsights.AspNetCore;
using AutoFixture;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;

namespace Allegro.Extensions.ApplicationInsights.Demo.UnitTests;

public class SamplingFilterTests : FilterTests<CustomDependencyForFilter, RequestForFilter>
{
    [Theory]
    [InlineData("GET Method", "skyfall", "Team eq 'skyfall'", true)]
    [InlineData("GET Method", "skyfall", "OperationName eq 'GET Method'", true)]
    public void Dependencies_Rules(string operationName, string team, string filter, bool shouldExclude)
    {
        var fixture = new Fixture();
        var dependency = new DependencyTelemetry(
            fixture.Create<string>(),
            fixture.Create<string>(),
            fixture.Create<string>(),
            null);
        dependency.Context.Operation.Name = operationName;
        dependency.Properties[nameof(CustomTelemetryCloudApplicationInfo.TeamName)] = team;

        CustomDependencyForFilter DependencyMap(DependencyTelemetry d) => new(d);

        ApplyDependencyRules(dependency, filter, DependencyMap);

        ((ISupportSampling)dependency).SamplingPercentage.Should().Be(shouldExclude ? 100 : null);
    }
}
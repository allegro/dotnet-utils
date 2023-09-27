using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Moq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.ApplicationInsights.AspNetCore.UnitTests;

public class SamplingFilterTests : FilterTests<DependencyForFilter, RequestForFilter>
{
    [Theory]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, null, "Type eq 'HTTP'", false)]
    [InlineData("custsrv", "HTTP", "Any target", "Any name", 150, true, null, "Type eq 'HTTP'", false)]
    [InlineData("bazolith", "Cosmos", "Any target", "Any name", 150, true, null, "Type eq 'HTTP'", true)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, null, "Target eq 'Any target'", false)]
    [InlineData("custsrv", "HTTP", "Any target", "Any name", 150, true, null, "Target eq 'Any target'", false)]
    [InlineData(
        "bazolith",
        "HTTP",
        "Any target 2",
        "Any name",
        150,
        true,
        null,
        "Target eq 'Any target'",
        true)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, null, "Name eq 'Any name'", false)]
    [InlineData("custsrv", "HTTP", "Any target", "Any name", 150, true, null, "Name eq 'Any name'", false)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name 2", 150, true, null, "Name eq 'Any name'", true)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, null, "Duration gt 100", false)]
    [InlineData("custsrv", "HTTP", "Any target", "Any name", 150, true, null, "Duration gt 100", false)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 80, true, null, "Duration gt 100", true)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, false, null, "Success eq false", false)]
    [InlineData("custsrv", "HTTP", "Any target", "Any name", 150, false, null, "Success eq false", false)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, null, "Success eq false", true)]
    [InlineData(
        "bazolith",
        "HTTP",
        "Any target",
        "Any name",
        150,
        true,
        null,
        "CloudRoleName eq 'bazolith'",
        false)]
    [InlineData(
        "bazolith",
        "Cosmos",
        "Any target",
        "Any name",
        150,
        true,
        null,
        "CloudRoleName eq 'bazolith'",
        false)]
    [InlineData(
        "custsrv",
        "HTTP",
        "Any target",
        "Any name",
        150,
        true,
        null,
        "CloudRoleName eq 'bazolith'",
        true)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, "200", "ResultCode eq '200'", false)]
    [InlineData("custsrv", "HTTP", "Any target", "Any name", 150, true, "200", "ResultCode eq '200'", false)]
    [InlineData(
        "bazolith",
        "HTTP",
        "Any target",
        "Any name",
        150,
        true,
        "Canceled",
        "ResultCode eq 'Canceled'",
        false)]
    [InlineData("bazolith", "HTTP", "Any target", "Any name", 150, true, "404", "ResultCode eq '200'", true)]
    [InlineData("bazolith", "Cosmos", "Any target", "Any name", 150, true, null, null, true)]
    [InlineData("bazolith", "Cosmos", "Any target", "Any name", 150, true, null, "", true)]
    [InlineData(null, "Cosmos", "Any target", "Any name", 150, true, null, "CloudRoleName eq 'SomeService'", true)]
    [InlineData(
        null,
        "Cosmos",
        "Any target",
        "Any name",
        150,
        true,
        null,
        "startswith(CloudRoleName, 'SomeService')",
        true)]
    [InlineData(null, null, "Any target", "Any name", 150, true, null, "Type eq 'Cosmos'", true)]
    [InlineData(null, null, "Any target", "Any name", 150, true, null, "startswith(Type, 'Cosmos')", true)]
    [InlineData(null, null, null, "Any name", 150, true, null, "Target eq 'Any target'", true)]
    [InlineData(null, null, null, "Any name", 150, true, null, "startswith(Target, 'Any target')", true)]
    [InlineData(null, null, null, null, 150, true, null, "Name eq 'Any name'", true)]
    [InlineData(null, null, null, null, 150, true, null, "startswith(Name, 'Any name')", true)]
    [InlineData(null, null, null, null, 150, true, null, "ResultCode eq '4'", true)]
    [InlineData(null, null, null, null, 150, true, null, "startswith(ResultCode, '4')", true)]
    public void Dependencies_Rules(
        string cloudRoleName,
        string type,
        string target,
        string name,
        int duration,
        bool success,
        string resultCode,
        string filter,
        bool shouldSample)
    {
        var dependency = new DependencyTelemetry(type, target, name, null);
        dependency.Duration = new TimeSpan(0, 0, 0, 0, duration);
        dependency.Context.Cloud.RoleName = cloudRoleName;
        dependency.ResultCode = resultCode;
        dependency.Success = success;

        DependencyForFilter DependencyMap(DependencyTelemetry d) => new(d);

        ApplyDependencyRules(dependency, filter, DependencyMap);

        ((ISupportSampling)dependency).SamplingPercentage.Should().Be(shouldSample ? null : 100);
    }

    [Theory]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 100, true, null, "Name eq 'GET Method'", false)]
    [InlineData("custsrv", "GET Method", "http://somehost/asd", 100, true, null, "Name eq 'GET Method'", false)]
    [InlineData("bazolith", "GET Method 2", "http://somehost/asd", 100, true, null, "Name eq 'GET Method'", true)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, true, null, "Duration gt 100", false)]
    [InlineData("custsrv", "GET Method", "http://somehost/asd", 150, true, null, "Duration gt 100", false)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 80, true, null, "Duration gt 100", true)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, false, null, "Success eq false", false)]
    [InlineData("custsrv", "GET Method", "http://somehost/asd", 150, false, null, "Success eq false", false)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, true, null, "Success eq false", true)]
    [InlineData(
        "bazolith",
        "GET Method",
        "http://somehost/asd",
        150,
        true,
        null,
        "CloudRoleName eq 'bazolith'",
        false)]
    [InlineData(
        "bazolith",
        "GET Method 2",
        "http://somehost/asd",
        150,
        true,
        null,
        "CloudRoleName eq 'bazolith'",
        false)]
    [InlineData(
        "custsrv",
        "GET Method",
        "http://somehost/asd",
        150,
        true,
        null,
        "CloudRoleName eq 'bazolith'",
        true)]
    [InlineData(
        "bazolith",
        "GET Method",
        "http://uniquehost/asd",
        150,
        true,
        null,
        "Url eq 'http://uniquehost/asd'",
        false)]
    [InlineData(
        "bazolith",
        "GET Method 2",
        "http://uniquehost/asd",
        150,
        true,
        null,
        "Url eq 'http://uniquehost/asd'",
        false)]
    [InlineData(
        "bazolith",
        "GET Method",
        "http://somehost/asd",
        150,
        true,
        null,
        "Url eq 'http://uniquehost/asd'",
        true)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, true, "200", "ResponseCode eq '200'", false)]
    [InlineData("custsrv", "GET Method", "http://somehost/asd", 150, true, "200", "ResponseCode eq '200'", false)]
    [InlineData(
        "bazolith",
        "GET Method",
        "http://somehost/asd",
        150,
        true,
        "Canceled",
        "ResponseCode eq 'Canceled'",
        false)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, true, "404", "ResponseCode eq '200'", true)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, true, null, null, true)]
    [InlineData("bazolith", "GET Method", "http://somehost/asd", 150, true, null, "", true)]
    [InlineData("bazolith", "GET Method", null, 150, true, null, "Url eq 'http://uniquehost/asd'", true)]
    [InlineData("bazolith", "GET Method", null, 150, true, null, "startswith(Url, 'http://uniquehost/asd')", true)]
    [InlineData("bazolith", null, null, 150, true, null, "Name eq 'GET Method'", true)]
    [InlineData("bazolith", null, null, 150, true, null, "startswith(Name, 'GET Method')", true)]
    [InlineData("bazolith", null, null, 150, true, null, "ResponseCode eq '401'", true)]
    [InlineData("bazolith", null, null, 150, true, null, "startswith(ResponseCode, '401')", true)]
    [InlineData(null, null, null, 150, true, null, "CloudRoleName eq 'SomeService'", true)]
    [InlineData(null, null, null, 150, true, null, "startswith(CloudRoleName, 'SomeService')", true)]
    public void Requests_Rules(
        string cloudRoleName,
        string name,
        string? url,
        int duration,
        bool success,
        string responseCode,
        string filter,
        bool shouldSample)
    {
        var requestTelemetry = new RequestTelemetry(
            name,
            DateTimeOffset.UtcNow,
            new TimeSpan(0, 0, 0, 0, duration),
            responseCode,
            success);
        requestTelemetry.Context.Cloud.RoleName = cloudRoleName;
        if (url != null)
        {
            requestTelemetry.Url = new Uri(url);
        }

        RequestForFilter RequestMap(RequestTelemetry d) => new(d);

        ApplyRequestRules(requestTelemetry, filter, RequestMap);

        ((ISupportSampling)requestTelemetry).SamplingPercentage.Should().Be(shouldSample ? null : 100);
    }

    [Theory]
    [InlineData("Cosmos", "Any target", "Any name", 150, true, "asd")]
    [InlineData("Cosmos", "Any target", "Any name", 150, true, "Type in ('HTTP', 'COS')")]
    [InlineData("Cosmos", "Any target", "Any name", 150, true, "NotExistingProperty eq 'HTTP'")]
    public void WhenFilterIsIncorrect_ShouldNotExcludeAndLogError(
        string type,
        string target,
        string name,
        int duration,
        bool success,
        string filter)
    {
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var dependency = new DependencyTelemetry(type, target, name, null);
        dependency.Duration = new TimeSpan(0, 0, 0, 0, duration);
        dependency.Success = success;
        var predicates =
            ExcludeFromSamplingTelemetryInitializer<DependencyForFilter, RequestForFilter>
                .CreatePredicates<DependencyForFilter>(
                    loggerMock.Object,
                    new Dictionary<string, string> { { "any", filter } });
        DependencyForFilter DependencyMap(DependencyTelemetry d) => new(d);

        ExcludeFromSamplingTelemetryInitializer<DependencyForFilter, RequestForFilter>.ApplyDependencyRules(
            dependency,
            predicates,
            DependencyMap);

        ((ISupportSampling)dependency).SamplingPercentage.Should().Be(null);

        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 2),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [AutoData]
    public void WhenFiltersAreEmpty_ShouldNotExclude(
        string type,
        string target,
        string name,
        int duration,
        bool success)
    {
        var loggerMock = new Mock<ILogger>();
        var dependency = new DependencyTelemetry(type, target, name, null);
        dependency.Duration = new TimeSpan(0, 0, 0, 0, duration);
        dependency.Success = success;
        var predicates =
            ExcludeFromSamplingTelemetryInitializer<DependencyForFilter, RequestForFilter>
                .CreatePredicates<DependencyForFilter>(
                    loggerMock.Object,
                    new Dictionary<string, string>());
        Func<DependencyTelemetry, DependencyForFilter> dependencyMap = d => new DependencyForFilter(d);

        ExcludeFromSamplingTelemetryInitializer<DependencyForFilter, RequestForFilter>.ApplyDependencyRules(
            dependency,
            predicates,
            dependencyMap);

        ((ISupportSampling)dependency).SamplingPercentage.Should().Be(null);
    }

    [Theory]
    [AutoData]
    public void WhenFiltersAreNull_ShouldNotExclude(
        string type,
        string target,
        string name,
        int duration,
        bool success)
    {
        var loggerMock = new Mock<ILogger>();
        var dependency = new DependencyTelemetry(type, target, name, null);
        dependency.Duration = new TimeSpan(0, 0, 0, 0, duration);
        dependency.Success = success;
        var predicates =
            ExcludeFromSamplingTelemetryInitializer<DependencyForFilter, RequestForFilter>
                .CreatePredicates<DependencyForFilter>(
                    loggerMock.Object,
                    null);
        Func<DependencyTelemetry, DependencyForFilter> dependencyMap = d => new DependencyForFilter(d);

        ExcludeFromSamplingTelemetryInitializer<DependencyForFilter, RequestForFilter>.ApplyDependencyRules(
            dependency,
            predicates,
            dependencyMap);

        ((ISupportSampling)dependency).SamplingPercentage.Should().Be(null);
    }
}
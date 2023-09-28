using System.Globalization;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Allegro.Extensions.ApplicationInsights.Prometheus.UnitTests;

public class ApplicationInsightsToPrometheusMetricsInitializerTests
{
    public ApplicationInsightsToPrometheusMetricsInitializerTests()
    {
        UrlCircuitBreaker.SetMaxUrisPerHost(100);
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData(null, true, false)]
    [InlineData("", false, false)]
    [InlineData("GET Something", false, false)]
    [InlineData("Anything else", false, false)]
    [InlineData("Process SomeTopic", false, false)]
    [InlineData("Process SomeTopic", true, true)]
    public void ApplyRequestsIncludes(string name, bool includeBus, bool shouldInclude)
    {
        var count = 0;
        var requestTelemetry = new RequestTelemetry(
            name,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMilliseconds(100),
            "200",
            true);

        ApplicationInsightsToPrometheusMetricsInitializer.ApplyRequestsIncludes(
            requestTelemetry,
            includeBus,
            (strings, d) => count++);

        count.Should().Be(shouldInclude ? 1 : 0);
    }

    [Theory]
    [InlineData("HTTP", new string[] { }, false)]
    [InlineData(null, new[] { "HTTP" }, false)]
    [InlineData("", new[] { "HTTP" }, false)]
    [InlineData("Some Type", new[] { "HTTP" }, false)]
    [InlineData("HTTP", new[] { "HTTP" }, true)]
    [InlineData("HTTP", new[] { "http" }, true)]
    [InlineData("http", new[] { "HTTP" }, true)]
    [InlineData("http", new[] { "http" }, true)]
    [InlineData("cosmos", new[] { "http", "cosmos" }, true)]
    [InlineData("Some Type", new[] { "http", "cosmos" }, false)]
    public void ApplyDependencyIncludes(string type, string[] typesToInclude, bool shouldInclude)
    {
        var count = 0;
        var dependencyTelemetry = new DependencyTelemetry(
            type,
            "Some target",
            "Some name",
            null);
        var typesToIncludeLower = typesToInclude.Select(p => p.ToLower(CultureInfo.InvariantCulture)).ToList();

        ApplicationInsightsToPrometheusMetricsInitializer.ApplyDependencyIncludes(
            dependencyTelemetry,
            typesToIncludeLower,
            false,
            false,
            (strings, d) => count++,
            NullLogger.Instance);

        count.Should().Be(shouldInclude ? 1 : 0);
    }

    [Theory]
    [InlineData("HTTP", "GET Method", new[] { "http", "cosmos" }, true, true, "GET Method")]
    [InlineData("HTTP", "Some Method", new[] { "http", "cosmos" }, true, true, "Some Method")]
    [InlineData("HTTP", "Some Method", new string[] { }, true, false, null)]
    [InlineData(
        "HTTP",
        "GET /agreements/v1/onboarding-short/asd1/asd2/Regular/asd3",
        new[] { "http", "cosmos" },
        true,
        true,
        "GET /agreements/v1/onboarding-short/asd1/asd2/Regular/asd3")]
    [InlineData(
        "HTTP",
        "GET /agreements/v1/onboarding-short/1880495135662745600/1880495135612091392/Regular/1231",
        new[] { "http", "cosmos" },
        true,
        true,
        "GET /agreements/v1/onboarding-short/{param}/{param}/Regular/{param}")]
    [InlineData(
        "HTTP",
        "GET /agreements/v1/onboarding-short/1880495135662745600/1880495135612091392/Regular/1231",
        new[] { "http", "cosmos" },
        false,
        true,
        "GET")]
    public void DependencyHttpNameResolved(
        string type,
        string name,
        string[] typesToInclude,
        bool shouldGeneralizeUrl,
        bool shouldInclude,
        string? nameExpected)
    {
        var count = 0;
        var labels = Array.Empty<string>();
        var dependencyTelemetry = new DependencyTelemetry(
            type,
            "Some target",
            name,
            null);
        var typesToIncludeLower = typesToInclude.Select(p => p.ToLower(CultureInfo.InvariantCulture)).ToList();
        var loggerMock = new Mock<ILogger>();

        ApplicationInsightsToPrometheusMetricsInitializer.ApplyDependencyIncludes(
            dependencyTelemetry,
            typesToIncludeLower,
            shouldGeneralizeUrl,
            true,
            (strings, d) =>
            {
                count++;
                labels = strings;
            },
            loggerMock.Object);

        count.Should().Be(shouldInclude ? 1 : 0);
        if (nameExpected != null)
        {
            labels.Should().Contain(nameExpected);
        }
        else
        {
            labels.Should().BeEmpty();
        }
    }

    [Fact]
    public void DependencyMetricsWasAddOnlyOnce()
    {
        var count = 0;
        var dependencyTelemetry = new DependencyTelemetry(
            "Some Type",
            "Some target",
            "Some name",
            null);
        var typesToIncludeLower = new List<string>() { "Some Type" }.Select(p => p.ToLower(CultureInfo.InvariantCulture)).ToList();
        ApplicationInsightsToPrometheusMetricsInitializer.ApplyDependencyIncludes(
            dependencyTelemetry,
            typesToIncludeLower,
            false,
            false,
            (strings, d) => { count++; },
            NullLogger.Instance);

        ApplicationInsightsToPrometheusMetricsInitializer.ApplyDependencyIncludes(
            dependencyTelemetry,
            typesToIncludeLower,
            false,
            false,
            (strings, d) => { count++; },
            NullLogger.Instance);

        count.Should().Be(1);
    }

    [Fact]
    public void RequestsMetricsWasAddOnlyOnce()
    {
        var count = 0;
        var requestTelemetry = new RequestTelemetry(
            "Process SomeTopic",
            DateTimeOffset.UtcNow,
            TimeSpan.FromMilliseconds(100),
            "200",
            true);
        ApplicationInsightsToPrometheusMetricsInitializer.ApplyRequestsIncludes(
            requestTelemetry,
            true,
            (strings, d) => count++);

        ApplicationInsightsToPrometheusMetricsInitializer.ApplyRequestsIncludes(
            requestTelemetry,
            true,
            (strings, d) => count++);

        count.Should().Be(1);
    }
}
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.ApplicationInsights.AspNetCore.UnitTests;

[SuppressMessage("CSharp Extensions", "CSE001:Required properties initialization", Justification = "SampleConfig is perfectly fine to init with default constructor")]
public class AdaptiveSamplingConfigurationTests
{
    [Fact]
    public void WhenConfigIsDefaultShouldReturnTelemetryProcessorWithConfigDefaultValues()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();

        var samplingTelemetryProcessor = ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(
            telemetryProcessorMock.Object,
            new SamplingConfig());

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenEvaluationIntervalIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig() { EvaluationInterval = TimeSpan.FromSeconds(5) }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(5));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenExcludedTypesIsChanged_ShouldChangeTelemetryProcessorValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig() { SamplingExcludedTypes = "Dependency" };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Dependency");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenInitialSamplingPercentageIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig() { InitialSamplingPercentage = 99 }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(99);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenMaxSamplingPercentageIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig() { MaxSamplingPercentage = 99 }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(99);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenMinSamplingPercentageIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig() { MinSamplingPercentage = 0.01 }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.01);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenMovingAverageRatioIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig() { MovingAverageRatio = 0.1 }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.1);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenMaxTelemetryItemsPerSecondIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig() { MaxTelemetryItemsPerSecond = 3 }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(3);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenSamplingPercentageDecreaseTimeoutIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig()
            {
                SamplingPercentageDecreaseTimeout = TimeSpan.FromMinutes(1)
            }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(1));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void WhenSamplingPercentageIncreaseTimeoutIsNotNull_ShouldOverwriteTelemetryProcessorDefaultValue()
    {
        var telemetryProcessorMock = new Mock<ITelemetryProcessor>();
        var config = new SamplingConfig()
        {
            AdaptiveSamplingConfig = new AdaptiveSamplingConfig()
            {
                SamplingPercentageIncreaseTimeout = TimeSpan.FromMinutes(5)
            }
        };

        var samplingTelemetryProcessor =
            ApplicationInsightsExtensions.CreateAdaptiveSamplingProcessor(telemetryProcessorMock.Object, config);

        samplingTelemetryProcessor.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(15));
        samplingTelemetryProcessor.ExcludedTypes.Should().Be("Event;Exception;Trace;PageView");
        samplingTelemetryProcessor.IncludedTypes.Should().BeNull();
        samplingTelemetryProcessor.InitialSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MaxSamplingPercentage.Should().Be(100);
        samplingTelemetryProcessor.MinSamplingPercentage.Should().Be(0.1);
        samplingTelemetryProcessor.MovingAverageRatio.Should().Be(0.25);
        samplingTelemetryProcessor.MaxTelemetryItemsPerSecond.Should().Be(5);
        samplingTelemetryProcessor.SamplingPercentageDecreaseTimeout.Should().Be(TimeSpan.FromMinutes(2));
        samplingTelemetryProcessor.SamplingPercentageIncreaseTimeout.Should().Be(TimeSpan.FromMinutes(5));
    }
}
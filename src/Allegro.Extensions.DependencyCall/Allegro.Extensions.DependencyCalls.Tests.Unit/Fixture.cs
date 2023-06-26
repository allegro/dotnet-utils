using System.Diagnostics;
using System.Reflection;
using Allegro.Extensions.DependencyCalls.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Prometheus;

namespace Allegro.Extensions.DependencyCalls.Tests.Unit;

internal class Fixture
{
    private readonly ServiceCollection _services;
    private readonly Assembly[] _applicationAssemblies;
    private ServiceProvider? _provider;
    private Mock<IDependencyCallMetrics>? _metricsSpy;
    private Action<DependencyCallBuilder>? _configureDependencyCall;

    public Fixture()
    {
        _services = new ServiceCollection();
        _applicationAssemblies = new[] { Assembly.GetExecutingAssembly() };
    }

    public Fixture Build()
    {
        _services.AddDependencyCall(
            applicationAssemblies: _applicationAssemblies,
            configureDependencyCall: builder =>
            {
                if (_metricsSpy is not null)
                {
                    builder.WithDependencyCallMetrics(_metricsSpy.Object);
                }

                _configureDependencyCall?.Invoke(builder);
            }).AddSingleton<IMetricFactory>(sp => Mock.Of<IMetricFactory>());
        _provider = _services.BuildServiceProvider();

        return this;
    }

    public IDependencyCallDispatcher Dispatcher => _provider!.GetRequiredService<IDependencyCallDispatcher>();

    public IDependencyCallMetrics DependencyCallMetrics => _provider!.GetRequiredService<IDependencyCallMetrics>();

    public Fixture WithConfiguration<T>(T testCallConfiguration) where T : class
    {
        _services.AddTransient<T>(sp => testCallConfiguration);
        return this;
    }

    public Fixture WithMetricsSpy()
    {
        var metricsSpy = new Mock<IDependencyCallMetrics>();
        _metricsSpy = metricsSpy;
        return this;
    }

    public Fixture WithBuilderAction(Action<DependencyCallBuilder> configureDependencyCall)
    {
        _configureDependencyCall = configureDependencyCall;
        return this;
    }

    public Fixture VerifyExecutedMetricsWereTriggered(Times times)
    {
        if (_metricsSpy is null)
        {
            throw new NotSupportedException("You need to build fixture with WithMetricsSpy option");
        }

        _metricsSpy.Verify(m => m.Succeeded(It.IsAny<IRequest>(), It.IsAny<Stopwatch>()), times);
        return this;
    }

    public Fixture VerifyFailedMetricsWereTriggered(Times times)
    {
        if (_metricsSpy is null)
        {
            throw new NotSupportedException("You need to build fixture with WithMetricsSpy option");
        }

        _metricsSpy.Verify(m => m.Failed(It.IsAny<IRequest>(), It.IsAny<Exception>(), It.IsAny<Stopwatch>()), times);
        return this;
    }

    public Fixture VerifyFallbackMetricsWereTriggered(Times times)
    {
        if (_metricsSpy is null)
        {
            throw new NotSupportedException("You need to build fixture with WithMetricsSpy option");
        }

        _metricsSpy.Verify(m => m.Fallback(It.IsAny<IRequest>(), It.IsAny<Exception>(), It.IsAny<Stopwatch>()), times);
        return this;
    }

    public Fixture VerifyNoOtherMetricsWereTriggered()
    {
        if (_metricsSpy is null)
        {
            throw new NotSupportedException("You need to build fixture with WithMetricsSpy option");
        }

        _metricsSpy.VerifyNoOtherCalls();
        return this;
    }
}
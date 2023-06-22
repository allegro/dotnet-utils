using Allegro.Extensions.DependencyCall.Abstractions;
using Allegro.Extensions.DependencyCall.Metrics.Prometheus;
using FluentAssertions;
using Moq;
using Polly;
using Xunit;

namespace Allegro.Extensions.DependencyCall.Tests.Unit;

public class DependencyCallDispatcherSpec
{
    public class DependencyCallPipeline
    {
        [Fact]
        public async Task Able_to_dispatch_call()
        {
            var testResponse = new TestResponse("testResponse");
            var fixture = new Fixture()
                .WithConfiguration(new TestCallConfiguration(testResponse))
                .Build();

            var response = await fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            response.Should().Be(testResponse);
        }

        [Fact]
        public async Task Able_to_dispatch_call_with_fallback_on_exception()
        {
            var testResponse = new TestResponse("testResponseFallback");
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(testResponse, ShouldThrowOnError: false, Exception: new TestException()))
                .Build();

            var response = await fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            response.Should().Be(testResponse);
        }

        [Fact]
        public async Task Able_to_dispatch_call_without_handling_error()
        {
            var testResponse = new TestResponse("testResponseFallback");
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(testResponse, ShouldThrowOnError: true, Exception: new TestException()))
                .Build();

            var act = () => fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            await act.Should().ThrowAsync<TestException>();
        }

        [Fact]
        public async Task Default_timeout_should_be_set_for_cacnellation()
        {
            var testResponse = new TestResponse("testResponseFallback");
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(
                        testResponse,
                        ShouldThrowOnError: true,
                        WaitingTimeInSeconds: 1,
                        DefaultTimeoutInMs: 100))
                .Build();
            var act = () => fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [Fact]
        public async Task Able_to_apply_custom_retry_policy()
        {
            var testResponse = new TestResponse("testResponseFallback");
            var customPolicyUsed = false;
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(
                        testResponse,
                        ShouldThrowOnError: true,
                        Exception: new TestException(),
                        CustomPolicy: Policy<TestResponse>.Handle<TestException>().FallbackAsync(
                            token =>
                            {
                                customPolicyUsed = true;
                                return Task.FromResult(testResponse);
                            })))
                .Build();
            var response = await fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            response.Should().Be(testResponse);
            customPolicyUsed.Should().BeTrue();
        }
    }

    public class DependencyCallMetrics
    {
        [Fact]
        public async Task Verify_metrics_triggered_on_success()
        {
            var fixture = new Fixture()
                .WithConfiguration(new TestCallConfiguration(new TestResponse("testResponse")))
                .WithMetricsSpy()
                .Build();

            await fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            fixture
                .VerifyCommonMetricsWereTriggered(Times.Once())
                .VerifyExecutedMetricsWereTriggered(Times.Once())
                .VerifyNoOtherMetricsWereTriggered();
        }

        [Fact]
        public async Task Verify_metrics_triggered_on_error_path()
        {
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(
                        new TestResponse("testResponse"),
                        ShouldThrowOnError: true,
                        Exception: new TestException()))
                .WithMetricsSpy()
                .Build();

            var act = () => fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            await act.Should().ThrowAsync<Exception>();

            fixture
                .VerifyCommonMetricsWereTriggered(Times.Once())
                .VerifyFailedMetricsWereTriggered(Times.Once())
                .VerifyNoOtherMetricsWereTriggered();
        }

        [Fact]
        public async Task Verify_metrics_triggered_on_error_path_with_fallback()
        {
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(
                        new TestResponse("testResponse"),
                        ShouldThrowOnError: false,
                        Exception: new TestException()))
                .WithMetricsSpy()
                .Build();

            await fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            fixture
                .VerifyCommonMetricsWereTriggered(Times.Once())
                .VerifyFallbackMetricsWereTriggered(Times.Once())
                .VerifyNoOtherMetricsWereTriggered();
        }
    }

    public class PrometheusBasedDependencyCallMetrics
    {
        [Fact]
        public void Able_to_register_prometheus_metrics()
        {
            var fixture = new Fixture()
                .WithBuilderAction(builder => builder.RegisterPrometheusDependencyCallMetrics("testApplicationName"))
                .Build();

            fixture.DependencyCallMetrics.Should().BeOfType<PrometheusDependencyCallMetrics>();
        }
    }

    private record TestCallConfiguration(
        TestResponse Response,
        bool ShouldThrowOnError = false,
        Exception? Exception = null,
        IAsyncPolicy<TestResponse>? CustomPolicy = null,
        int WaitingTimeInSeconds = 0,
        int? DefaultTimeoutInMs = null);

    private class TestCall : DependencyCall<TestRequest, TestResponse>
    {
        private readonly TestCallConfiguration _configuration;

        public TestCall(TestCallConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task<TestResponse> Execute(TestRequest request, CancellationToken cancellationToken)
        {
            if (_configuration.Exception is not null)
            {
                throw _configuration.Exception;
            }

            await Task.Delay(TimeSpan.FromSeconds(_configuration.WaitingTimeInSeconds), cancellationToken);
            return _configuration.Response;
        }

        protected override Task<(ShouldThrowOnError ShouldThrowOnError, TestResponse Response)> Fallback(
            TestRequest request,
            Exception exception,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                (
                    ShouldThrowOnError: _configuration.ShouldThrowOnError
                        ? ShouldThrowOnError.Yes
                        : ShouldThrowOnError.No,
                    Response: _configuration.Response
                ));
        }

        protected override TimeSpan CancelAfter => _configuration.DefaultTimeoutInMs is null
            ? base.CancelAfter
            : TimeSpan.FromMilliseconds(_configuration.DefaultTimeoutInMs.Value);

        protected override IAsyncPolicy<TestResponse> CustomPolicy(CancellationToken cancellationToken)
        {
            return _configuration.CustomPolicy is null
                ? base.CustomPolicy(cancellationToken)
                : _configuration.CustomPolicy;
        }
    }

    private class TestException : Exception
    {
    }

    private record TestRequest(string Data) : IRequest<TestResponse>;

    private record TestResponse(string Data);
}
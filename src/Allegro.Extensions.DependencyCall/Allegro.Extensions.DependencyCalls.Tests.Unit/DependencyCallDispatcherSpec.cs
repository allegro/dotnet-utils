using Allegro.Extensions.DependencyCalls.Abstractions;
using Allegro.Extensions.DependencyCalls.Exceptions;
using Allegro.Extensions.DependencyCalls.Metrics.Prometheus;
using Allegro.Extensions.DependencyCalls.Polly;
using FluentAssertions;
using Moq;
using Polly;
using Polly.Timeout;
using Xunit;

namespace Allegro.Extensions.DependencyCalls.Tests.Unit;

#pragma warning disable CS1591
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
        public async Task Able_to_set_timeout()
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
            var act = () => fixture.Dispatcher.Dispatch(new TestRequestTimeout("testRequest"));

            await act.Should().ThrowAsync<TimeoutRejectedException>();
        }

        [Fact]
        public async Task Not_execute_if_cancellation_token_cancelled()
        {
            var testResponse = new TestResponse("testResponseFallback");
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(
                        testResponse,
                        ShouldThrowOnError: true))
                .Build();
            var cancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;

            var act = () => fixture.Dispatcher.Dispatch(new TestRequest("testRequest"), cancellationToken);

            await act.Should().ThrowAsync<OperationCanceledException>();
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
            var response = await fixture.Dispatcher.Dispatch(new TestRequestCustomPolicy("testRequest"));

            response.Should().Be(testResponse);
            customPolicyUsed.Should().BeTrue();
        }

        [Fact]
        public async Task On_fallback_error_wrapped_excpetion_should_be_thrown()
        {
            var testResponse = new TestResponse("testResponseFallback");
            var fixture = new Fixture()
                .WithConfiguration(
                    new TestCallConfiguration(
                        testResponse,
                        ShouldThrowOnError: true,
                        Exception: new TestException(),
                        FallbackException: new TestException()))
                .Build();
            var act = () => fixture.Dispatcher.Dispatch(new TestRequest("testRequest"));

            await act.Should().ThrowAsync<FallbackExecutionException>();
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
        int? DefaultTimeoutInMs = null,
        Exception? FallbackException = null);

    private abstract class TestCallBase<TRequest> : PollyDependencyCall<TRequest, TestResponse>
        where TRequest : IRequest<TestResponse>
    {
        private readonly TestCallConfiguration _configuration;

        protected TestCallBase(TestCallConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override async Task<TestResponse> Execute(TRequest request, CancellationToken cancellationToken)
        {
            if (_configuration.Exception is not null)
            {
                throw _configuration.Exception;
            }

            await Task.Delay(TimeSpan.FromSeconds(_configuration.WaitingTimeInSeconds), cancellationToken);
            return _configuration.Response;
        }

        public override Task<TestResponse> Fallback(
            TRequest request,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (_configuration.FallbackException is not null)
            {
                throw _configuration.FallbackException;
            }

            return Task.FromResult(
                _configuration.ShouldThrowOnError
                    ? throw new TestFallbackException()
                    : _configuration.Response);
        }
    }

    private class TestCall : TestCallBase<TestRequest>
    {
        public TestCall(TestCallConfiguration configuration) : base(configuration)
        {
        }
    }

    private class TestCallTimeout : TestCallBase<TestRequestTimeout>
    {
        private readonly TestCallConfiguration _configuration;

        public TestCallTimeout(TestCallConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        public override PollyPolicyConfiguration Configuration =>
            _configuration.DefaultTimeoutInMs is null
                ? PollyPolicyConfiguration.Default
                : new PollyPolicyConfiguration(
                    TimeSpan.FromMilliseconds(_configuration.DefaultTimeoutInMs.Value));
    }

    private class TestCallCustomPolicy : TestCallBase<TestRequestCustomPolicy>
    {
        private readonly TestCallConfiguration _configuration;

        public TestCallCustomPolicy(TestCallConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        public override IAsyncPolicy<TestResponse>[]? CustomResultPolicies =>
            _configuration.CustomPolicy is not null ? new[] { _configuration.CustomPolicy } : null;
    }

    private class TestException : Exception
    {
    }

    private class TestFallbackException : Exception
    {
    }

    private record TestRequest(string Data) : IRequest<TestResponse>;

    private record TestRequestTimeout(string Data) : IRequest<TestResponse>;

    private record TestRequestCustomPolicy(string Data) : IRequest<TestResponse>;

    private record TestResponse(string Data);
}
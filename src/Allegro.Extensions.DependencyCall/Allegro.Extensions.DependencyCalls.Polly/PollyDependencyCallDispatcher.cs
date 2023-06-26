using System.Diagnostics;
using Allegro.Extensions.DependencyCalls.Abstractions;
using Allegro.Extensions.DependencyCalls.Exceptions;
using Allegro.Extensions.DependencyCalls.Polly.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Fallback;
using Polly.Timeout;

// ReSharper disable ArgumentsStyleNamedExpression
namespace Allegro.Extensions.DependencyCalls.Polly;

// Consider if it should be possible to run multiple dispatchers sententiously.
internal class PollyDependencyCallDispatcher : IDependencyCallDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDependencyCallMetrics _dependencyCallMetrics;

    public PollyDependencyCallDispatcher(IServiceProvider serviceProvider, IDependencyCallMetrics dependencyCallMetrics)
    {
        _serviceProvider = serviceProvider;
        _dependencyCallMetrics = dependencyCallMetrics;
    }

    public async Task<TResult> Dispatch<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : Request<TResult>
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        // Cannot use GetService<DependencyCall<TRequest>() as there's is underlying TResult.
        var dependencyCallType = typeof(DependencyCall<,>).MakeGenericType(request.GetType(), typeof(TResult));
        var dependencyCall = scope.ServiceProvider.GetService(dependencyCallType);

        if (dependencyCall is null)
        {
            throw new MissingDependencyCallException(dependencyCallType);
        }

        return dependencyCall switch
        {
            PollyDependencyCall<TRequest, TResult> pollyDependencyCall =>
                await Run(pollyDependencyCall, request, cancellationToken),
            _ => throw new MissingDependencyCallException("Missing non polly runner")
        };
    }

    private async Task<TResult> Run<TRequest, TResult>(
        PollyDependencyCall<TRequest, TResult> dependencyCall,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : Request<TResult>
    {
        var dependencyCallTimer = new Stopwatch();

        var timeoutPolicy = CreateTimeoutPolicy<TRequest, TResult>(
            dependencyCall.Configuration.Timeout,
            dependencyCall.Configuration.TimeoutStrategy,
            request,
            dependencyCallTimer);

        var fallBackPolicy = CreateFallbackPolicy(
            dependencyCall.Fallback<TResult>,
            request,
            dependencyCallTimer);

        var policies = fallBackPolicy.WrapAsync(timeoutPolicy);

        if (dependencyCall.CustomPolicies?.Any() is true)
        {
            policies.WrapAsync(Policy.WrapAsync(dependencyCall.CustomPolicies));
        }

        dependencyCallTimer.Start();
        var result = await policies.ExecuteAsync(
            async pollyCancellationToken =>
            {
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    pollyCancellationToken,
                    cancellationToken);
                return await dependencyCall.Execute<TResult>(request, linkedTokenSource.Token);
            },
            CancellationToken.None);

        // CancellationToken.None above here indicates missing independent cancellation control and
        // cancellation will be provided by TimeoutPolicy.
        dependencyCallTimer.Stop();
        _dependencyCallMetrics.Succeeded<TRequest, TResult>(request, dependencyCallTimer);
        return result;
    }

    private AsyncFallbackPolicy<TResult> CreateFallbackPolicy<TRequest, TResult>(
        Func<TRequest, Exception, CancellationToken, Task<TResult>> fallback,
        TRequest request,
        Stopwatch dependencyCallTimer)
        where TRequest : Request<TResult>
    {
        return Policy<TResult>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: async (result, _, cancellationToken) =>
                {
                    try
                    {
                        var fallbackResult = await fallback(request, result.Exception, cancellationToken);
                        _dependencyCallMetrics.Fallback<TRequest, TResult>(request, result.Exception, dependencyCallTimer);
                        return fallbackResult;
                    }
                    catch
                    {
                        _dependencyCallMetrics.Failed<TRequest, TResult>(request, result.Exception, dependencyCallTimer);
                        throw new FallbackExecutionException(request.GetType(), result.Exception);
                    }
                },
                onFallbackAsync: (_, _) => Task.CompletedTask); // The action to call asynchronously before invoking the fallback delegate
    }

    private AsyncTimeoutPolicy CreateTimeoutPolicy<TRequest, TResult>(
        TimeSpan timeout,
        TimeoutStrategy timeoutStrategy,
        TRequest request,
        Stopwatch dependencyCallTimer)
        where TRequest : Request<TResult>
    {
        return Policy
            .TimeoutAsync(
                timeout: timeout,
                timeoutStrategy: timeoutStrategy,
                onTimeoutAsync: (_, _, _) =>
                {
                    _dependencyCallMetrics.Timeout<TRequest, TResult>(request, dependencyCallTimer);
                    return Task.CompletedTask;
                });
    }
}
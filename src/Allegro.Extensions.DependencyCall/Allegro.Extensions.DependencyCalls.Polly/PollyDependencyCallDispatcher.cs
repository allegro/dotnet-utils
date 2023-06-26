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

    public async Task<TResponse> Dispatch<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        // Cannot use GetService<IPollyDependencyCall<IRequest<TResponse>, TResponse>>() as there's is underlying TResponse.
        var dependencyCallType = typeof(IPollyDependencyCall<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var dependencyCall = scope.ServiceProvider.GetService(dependencyCallType);

        if (dependencyCall is null)
        {
            throw new MissingDependencyCallException(dependencyCallType);
        }

        var pollyDependencyCall = dependencyCall as IPollyDependencyCall<IRequest<TResponse>, TResponse>; // TODO: this is broken
        return await Run(pollyDependencyCall!, request, cancellationToken);
    }

    private async Task<TResponse> Run<TResponse>(
        IPollyDependencyCall<IRequest<TResponse>, TResponse> dependencyCall,
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        var dependencyCallTimer = new Stopwatch();

        var timeoutPolicy = CreateTimeoutPolicy(
            dependencyCall.Configuration.Timeout,
            dependencyCall.Configuration.TimeoutStrategy,
            request,
            dependencyCallTimer);

        var fallBackPolicy = CreateFallbackPolicy(
            dependencyCall.Fallback,
            request,
            dependencyCallTimer);

        var policies = fallBackPolicy.WrapAsync(timeoutPolicy);

        if (dependencyCall.CustomPolicies?.Any() is true)
        {
            policies = policies.WrapAsync(Policy.WrapAsync(dependencyCall.CustomPolicies));
        }

        if (dependencyCall.CustomResultPolicies?.Any() is true)
        {
            policies = policies.WrapAsync(Policy.WrapAsync(dependencyCall.CustomResultPolicies));
        }

        dependencyCallTimer.Start();
        var response = await policies.ExecuteAsync(
            async pollyCancellationToken =>
            {
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    pollyCancellationToken,
                    cancellationToken);
                return await dependencyCall.Execute(request, linkedTokenSource.Token);
            },
            CancellationToken.None);

        // CancellationToken.None above here indicates missing independent cancellation control and
        // cancellation will be provided by TimeoutPolicy.
        dependencyCallTimer.Stop();
        _dependencyCallMetrics.Succeeded(request, dependencyCallTimer);
        return response;
    }

    private AsyncFallbackPolicy<TResponse> CreateFallbackPolicy<TRequest, TResponse>(
        Func<TRequest, Exception, CancellationToken, Task<TResponse>> fallback,
        TRequest request,
        Stopwatch dependencyCallTimer)
        where TRequest : IRequest<TResponse>
    {
        return Policy<TResponse>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: async (result, _, cancellationToken) =>
                {
                    try
                    {
                        var fallbackResponse = await fallback(request, result.Exception, cancellationToken);
                        _dependencyCallMetrics.Fallback(request, result.Exception, dependencyCallTimer);
                        return fallbackResponse;
                    }
                    catch
                    {
                        _dependencyCallMetrics.Failed(request, result.Exception, dependencyCallTimer);
                        throw new FallbackExecutionException(request.GetType(), result.Exception);
                    }
                },
                onFallbackAsync: (_, _) => Task.CompletedTask); // The action to call asynchronously before invoking the fallback delegate
    }

    private AsyncTimeoutPolicy CreateTimeoutPolicy<TResponse>(
        TimeSpan timeout,
        TimeoutStrategy timeoutStrategy,
        IRequest<TResponse> request,
        Stopwatch dependencyCallTimer)
    {
        return Policy
            .TimeoutAsync(
                timeout: timeout,
                timeoutStrategy: timeoutStrategy,
                onTimeoutAsync: (_, _, _) =>
                {
                    _dependencyCallMetrics.Timeout(request, dependencyCallTimer);
                    return Task.CompletedTask;
                });
    }
}
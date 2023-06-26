using System.Reflection;
using Allegro.Extensions.DependencyCalls.Abstractions;
using Allegro.Extensions.DependencyCalls.Exceptions;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArgumentsStyleNamedExpression
namespace Allegro.Extensions.DependencyCalls;

// Consider if it should be possible to run multiple dispatchers sententiously.
internal class DefaultDependencyCallDispatcher : IDependencyCallDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultDependencyCallDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task<TResponse> Dispatch<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();

        var handlerType = typeof(IDependencyCall<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler is null)
        {
            // TODO: throw this on startup
            throw new MissingDependencyCallException<TResponse>(request);
        }

        var handleMethodInfo = handlerType
            .GetMethod(nameof(IDependencyCall<IRequest<TResponse>, TResponse>.Run));

        if (handleMethodInfo is null)
        {
            throw new MissingMethodException($"Missing method Run in dependency call for request {request.GetType().FullName}");
        }

        var dependencyCallMetrics = scope.ServiceProvider.GetRequiredService<IDependencyCallMetrics>();

        var invoke = handleMethodInfo.Invoke(
            handler,
            BindingFlags.DoNotWrapExceptions,
            binder: null,
            new object?[] { request, dependencyCallMetrics, cancellationToken },
            culture: null)!;

        return await (Task<TResponse>)invoke;
    }
}
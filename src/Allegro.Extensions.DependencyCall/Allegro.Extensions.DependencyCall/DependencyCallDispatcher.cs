using System.Reflection;
using Allegro.Extensions.DependencyCall.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.DependencyCall;

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
            // TODO: consider kind of validation on startup rather on runtime
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
            null,
            new object?[] { request, dependencyCallMetrics, cancellationToken },
            null)!;

        return await (Task<TResponse>)invoke;
    }
}

internal class MissingDependencyCallException<T> : MissingDependencyCallException
{
    public MissingDependencyCallException(IRequest<T> query) : base($"Missing handler for query {query.GetType().FullName}")
    {
    }
}

internal class MissingDependencyCallException : Exception
{
    public MissingDependencyCallException(string message) : base(message)
    {
    }
}
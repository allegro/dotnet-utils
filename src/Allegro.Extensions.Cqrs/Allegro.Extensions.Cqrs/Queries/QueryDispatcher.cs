using System;
using System.Threading;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Queries;

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task<TResult?> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        // TODO: maybe some configuration to reuse outer scope instead of creating new one
        using var scope = _serviceProvider.CreateScope();

        // TODO: micro-optimization possibility - cache those types
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler is null)
        {
            // TODO: throw this on startup
            throw new MissingQueryHandlerException<TResult>(query);
        }

        var handleMethodInfo = handlerType
            .GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.Handle));

        var invoke = handleMethodInfo?.Invoke(handler, new object?[] { query, cancellationToken });

        if (invoke is null)
        {
            return default;
        }

        return await (Task<TResult?>)invoke;
    }
}

internal class MissingQueryHandlerException<T> : MissingQueryHandlerException
{
    public MissingQueryHandlerException(IQuery<T> query) : base($"Missing handler for query {query.GetType().FullName}")
    {
    }
}

internal class MissingQueryHandlerException : Exception
{
    public MissingQueryHandlerException(string message) : base(message)
    {
    }
}
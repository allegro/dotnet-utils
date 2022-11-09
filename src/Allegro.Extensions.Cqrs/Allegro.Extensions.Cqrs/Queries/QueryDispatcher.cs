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
        using var scope = _serviceProvider.CreateScope();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler is null)
        {
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
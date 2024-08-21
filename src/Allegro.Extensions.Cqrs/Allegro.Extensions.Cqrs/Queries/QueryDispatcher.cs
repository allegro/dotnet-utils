using System.Reflection;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Queries;

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task<TResult> Query<TResult>(Query<TResult> query, CancellationToken cancellationToken)
    {
        // TODO: maybe some configuration to reuse outer scope instead of creating new one
        using var scope = _serviceProvider.CreateScope();

        var validatorType = typeof(IQueryValidator<>).MakeGenericType(query.GetType());
        var queryValidators = scope.ServiceProvider.GetServices(validatorType);

        var validateMethodInfo = validatorType
            .GetMethod(nameof(IQueryValidator<Query<TResult>>.Validate));

        if (validateMethodInfo is null)
        {
            throw new MissingMethodException($"Missing method Validate in validator for query {query.GetType().FullName}");
        }

        // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/7.0/reflection-invoke-exceptions

        await Task.WhenAll(queryValidators.Select(p =>
        {
            // this is not a constructor, we can skip null
            var invoke = validateMethodInfo.Invoke(
                p,
                BindingFlags.DoNotWrapExceptions,
                null,
                new object?[] { query, cancellationToken },
                null)!;

            return (Task)invoke;
        }));

        // TODO: micro-optimization possibility - cache those types
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handlers = scope.ServiceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
            throw new MissingQueryHandlerException<TResult>(query);

        if (handlers.Count > 1)
            throw new MultipleQueryHandlerException<TResult>(query);

        var handler = handlers.Single();

        var handleMethodInfo = handlerType
            .GetMethod(nameof(IQueryHandler<Query<TResult>, TResult>.Handle));

        if (handleMethodInfo is null)
            throw new MissingMethodException($"Missing method Handle in handler for query {query.GetType().FullName}");

        // this is not a constructor, we can skip null
        var invoke = handleMethodInfo.Invoke(
            handler,
            BindingFlags.DoNotWrapExceptions,
            null,
            new object?[] { query, cancellationToken },
            null)!;

        return await (Task<TResult>)invoke;
    }
}

internal class MissingQueryHandlerException<T> : MissingQueryHandlerException
{
    public MissingQueryHandlerException(Query<T> query) : base($"Missing handler for query {query.GetType().FullName}")
    {
    }
}

internal class MultipleQueryHandlerException<T> : Exception
{
    public MultipleQueryHandlerException(Query<T> query)
        : base($"Multiple handler for query {query.GetType().FullName}")
    {
    }
}

internal class MissingQueryHandlerException : Exception
{
    public MissingQueryHandlerException(string message) : base(message)
    {
    }
}
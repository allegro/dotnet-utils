using System;
using System.Linq;
using System.Reflection;
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

    public async Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        // TODO: maybe some configuration to reuse outer scope instead of creating new one
        using var scope = _serviceProvider.CreateScope();

        var validatorType = typeof(IQueryValidator<>).MakeGenericType(query.GetType());
        var queryValidators = scope.ServiceProvider.GetServices(validatorType);

        var validateMethodInfo = validatorType
            .GetMethod(nameof(IQueryValidator<IQuery<TResult>>.Validate));

        if (validateMethodInfo is null)
        {
            throw new Exception($"Missing method Validate in validator for query {query.GetType().FullName}");
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
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler is null)
        {
            // TODO: throw this on startup
            throw new MissingQueryHandlerException<TResult>(query);
        }

        var handleMethodInfo = handlerType
            .GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.Handle));

        if (handleMethodInfo is null)
        {
            throw new Exception($"Missing method Handle in handler for query {query.GetType().FullName}");
        }

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
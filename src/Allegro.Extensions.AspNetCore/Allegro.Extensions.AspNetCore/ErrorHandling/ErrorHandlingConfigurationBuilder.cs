using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.AspNetCore.ErrorHandling;

/// <summary>
/// Error Handling Configuration Builder
/// </summary>
public class ErrorHandlingConfigurationBuilder
{
    internal IDictionary<Type, Func<Exception, Error>> CustomErrorHandlerMap { get; }
    internal ICollection<Func<HttpContext, IDisposable>> AdditionalInstrumentation { get; }
    internal Func<ActionContext, Error?>? CustomModelStateValidationErrorHandler { get; private set; }

    internal ErrorHandlingConfigurationBuilder()
    {
        CustomErrorHandlerMap = new Dictionary<Type, Func<Exception, Error>>();
        AdditionalInstrumentation = new List<Func<HttpContext, IDisposable>>();
    }

    /// <summary>
    /// Enables to setup custom handling of TException type
    /// </summary>
    /// <param name="handler">Defines how TException should be handled by middleware</param>
    public ErrorHandlingConfigurationBuilder WithCustomHandler<TException>(
        Func<TException, Error> handler)
        where TException : Exception
        => AddHandler(handler);

    /// <summary>
    /// Add support to configure additional instrumentation - for example extend logs with some custom attributes - for each occur of error
    /// </summary>
    public ErrorHandlingConfigurationBuilder WithAdditionalInstrumentation(
        Func<HttpContext, IDisposable> additionalInstrumentation)
        => AddAdditionalInstrumentation(additionalInstrumentation);

    /// <summary>
    /// Enables to setup custom handling of api endpoint binding issue (BadRequests)
    /// </summary>
    /// <param name="handler">Defines how custom validation error should be handled</param>
    public ErrorHandlingConfigurationBuilder WithCustomModelStateValidationErrorHandler(
        Func<ActionContext, Error?> handler)
    {
        CustomModelStateValidationErrorHandler = handler;
        return this;
    }

    private ErrorHandlingConfigurationBuilder AddHandler<TException>(
        Func<TException, Error> handler)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (!CustomErrorHandlerMap.TryAdd(typeof(TException), TypedHandlerWrapper(handler)))
        {
            throw new ArgumentException(
                $"CustomErrorHandler for type {typeof(TException).Name} already registered.");
        }

        return this;
    }

    private ErrorHandlingConfigurationBuilder AddAdditionalInstrumentation(
        Func<HttpContext, IDisposable> additionalInstrumentation)
    {
        ArgumentNullException.ThrowIfNull(additionalInstrumentation);

        AdditionalInstrumentation.Add(additionalInstrumentation);

        return this;
    }

    private static Func<Exception, Error> TypedHandlerWrapper<TException>(
        Func<TException, Error> handler)
        where TException : Exception
    {
        return exception => handler((exception as TException)!);
    }
}
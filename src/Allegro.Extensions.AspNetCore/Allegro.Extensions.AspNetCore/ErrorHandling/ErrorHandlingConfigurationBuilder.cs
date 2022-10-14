using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.AspNetCore.ErrorHandling;

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

    public ErrorHandlingConfigurationBuilder WithCustomHandler<TException>(
        Func<TException, Error> handler)
        where TException : Exception
        => AddHandler(handler);

    public ErrorHandlingConfigurationBuilder WithAdditionalInstrumentation(
        Func<HttpContext, IDisposable> additionalInstrumentation)
        => AddAdditionalInstrumentation(additionalInstrumentation);

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
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

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
        if (additionalInstrumentation == null)
        {
            throw new ArgumentNullException(nameof(additionalInstrumentation));
        }

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
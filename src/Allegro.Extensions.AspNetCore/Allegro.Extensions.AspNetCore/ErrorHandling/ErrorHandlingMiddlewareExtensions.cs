using System;
using Allegro.Extensions.AspNetCore.ErrorHandling.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Allegro.Extensions.AspNetCore.ErrorHandling;

public static class ErrorHandlingMiddlewareExtensions
{
    public static IServiceCollection AddFluentErrorHandlingMiddleware(
        this IServiceCollection services,
        Action<(string Message, Exception Exception)> logError,
        Action<(string Message, Exception Exception)> logWarning,
        Action<ErrorHandlingConfigurationBuilder>? customErrorHandlerRegistration = null,
        IErrorSerializer? errorSerializer = null)
    {
        var errorHandlingConfigurationBuilder = CreateErrorHandlingConfigurationBuilder(customErrorHandlerRegistration);

        services.TryAddSingleton(
            provider => new ErrorHandlingMiddleware(
                errorHandlingConfigurationBuilder.CustomErrorHandlerMap,
                errorHandlingConfigurationBuilder.AdditionalInstrumentation,
                errorSerializer ?? new SystemTextJsonWebErrorSerializer(),
                logError,
                logWarning));
        return services;
    }

    private static ErrorHandlingConfigurationBuilder CreateErrorHandlingConfigurationBuilder(
        Action<ErrorHandlingConfigurationBuilder>? customErrorHandlerRegistration)
    {
        var errorHandlingConfigurationBuilder = new ErrorHandlingConfigurationBuilder();

        customErrorHandlerRegistration?.Invoke(errorHandlingConfigurationBuilder);
        return errorHandlingConfigurationBuilder;
    }

    public static IMvcBuilder AddFluentModelStateValidationHandling(
        this IMvcBuilder mvcBuilder,
        Action<ErrorHandlingConfigurationBuilder>? customErrorHandlerRegistration = null)
    {
        var errorHandlingConfigurationBuilder = CreateErrorHandlingConfigurationBuilder(customErrorHandlerRegistration);

        var invalidModelStateResponseFactory =
            new InvalidModelStateResponseFactory(errorHandlingConfigurationBuilder.CustomModelStateValidationErrorHandler);
        mvcBuilder
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    invalidModelStateResponseFactory.BuildResponse(context);
            });

        return mvcBuilder;
    }

    public static IApplicationBuilder UseFluentErrorHandlingMiddleware(
        this IApplicationBuilder app,
        params object[] args)
        => app.UseMiddleware<ErrorHandlingMiddleware>(args);
}
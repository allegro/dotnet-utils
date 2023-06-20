using Allegro.Extensions.AspNetCore.ErrorHandling.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Allegro.Extensions.AspNetCore.ErrorHandling;

/// <summary>
/// Error Handling Middleware Extensions
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds fluent error handling support to application
    /// </summary>
    /// <param name="services">Service collections</param>
    /// <param name="logError">Delegate how log errors - enables to use any logger technology without Microsoft ILogger/<T/></param>
    /// <param name="logWarning">Delegate how log warnings - enables to use any logger technology without Microsoft ILogger/<T/></param>
    /// <param name="customErrorHandlerRegistration">Custom handler configurations</param>
    /// <param name="errorSerializer">Enables to use other kind of error serialization than default System.Text.Json in Web mode</param>
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

    /// <summary>
    /// Add fluent model state validation handling support of requets
    /// </summary>
    /// <param name="mvcBuilder">Mvc builder</param>
    /// <param name="customErrorHandlerRegistration">Custom validation confgiruation</param>
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

    /// <summary>
    /// Use fluent error handling middleware in pipeline
    /// </summary>
    public static IApplicationBuilder UseFluentErrorHandlingMiddleware(
        this IApplicationBuilder app,
        params object[] args)
        => app.UseMiddleware<ErrorHandlingMiddleware>(args);
}
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Exceptions;
using Allegro.Extensions.Configuration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Configuration.Extensions;

public static class ConfeatureAppBuilderExtensions
{
    private const string IdempotencyKey = nameof(UseConfeature);

    private static readonly PathString ConfigurationPathString = new("/configuration");
    private static readonly PathString ConfigurationProviderPathString = new("/configuration/provider");

    /// <summary>
    /// Configures Confeature middleware that serves the configuration endpoint.
    /// This invocation is optional - when not called, the Confeature middleware will still be injected as the last
    /// middleware. Use this method to add the middleware higher in the middlewares' order.
    /// </summary>
    /// <param name="appBuilder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
    /// <returns>The original <see cref="IApplicationBuilder" /> instance.</returns>
    public static IApplicationBuilder UseConfeature(this IApplicationBuilder appBuilder)
    {
        if (appBuilder.Properties.ContainsKey(IdempotencyKey))
        {
            return appBuilder;
        }

        var confeatureOptions = appBuilder.ApplicationServices.GetRequiredService<IOptions<ConfeatureOptions>>();
        if (!confeatureOptions.Value.IsEnabled)
        {
            return appBuilder;
        }

        OptionsRegistrationValidator.Validate(appBuilder.ApplicationServices);

        appBuilder.Properties[IdempotencyKey] = true;

        return appBuilder
            .MapWhen(
                ctx => IsValidConfeatureRequestPath(ctx, ConfigurationProviderPathString),
                app => app.UseMiddleware<ConfeaturePerProviderMiddleware>())
            .MapWhen(
                ctx => IsValidConfeatureRequestPath(ctx, ConfigurationPathString),
                app => app.UseMiddleware<ConfeatureGenericMiddleware>());
    }

    private static bool IsValidConfeatureRequestPath(HttpContext ctx, string confeaturePath)
        => ctx.Request.Path.StartsWithSegments(
               confeaturePath,
               StringComparison.OrdinalIgnoreCase,
               out var remaining) &&
           (!remaining.HasValue || remaining.Value == "/");
}

internal class ConfeaturePerProviderMiddleware
{
    public ConfeaturePerProviderMiddleware(RequestDelegate next)
    {
    }

    public static async Task InvokeAsync(
        HttpContext context,
        IConfigurationPrinter configurationPrinter)
    {
        if (!await ConfeatureMiddlewareHelper.IsAuthorized(context))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var providerContent = configurationPrinter.GetRawProviderContent(
            context.Request.Query["type"],
            context.Request.Query["key"]);

        if (providerContent is null)
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        else
            await context.Response.WriteAsync(providerContent, context.RequestAborted);
    }
}

internal class ConfeatureGenericMiddleware
{
    public ConfeatureGenericMiddleware(RequestDelegate next)
    {
    }

    public static async Task InvokeAsync(
        HttpContext context,
        IConfigurationPrinter configurationPrinter)
    {
        if (!await ConfeatureMiddlewareHelper.IsAuthorized(context))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await context.Response.WriteAsJsonAsync(configurationPrinter.GetConfiguration(), context.RequestAborted);
    }
}

internal static class ConfeatureMiddlewareHelper
{
    internal static async Task<bool> IsAuthorized(HttpContext context)
    {
        var hostEnvironment = context.RequestServices.GetService<IHostEnvironment>();
        if (hostEnvironment?.IsDevelopment() == true)
        {
            // do not authorize on local env / tests
            return true;
        }

        var confeatureOptions = context.RequestServices.GetRequiredService<IOptions<ConfeatureOptions>>();
        if (confeatureOptions.Value.AuthorizationPolicy == null)
        {
            return true;
        }

        var policyProvider = context.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();
        var policyEvaluator = context.RequestServices.GetRequiredService<IPolicyEvaluator>();
        var policy = await policyProvider.GetPolicyAsync(confeatureOptions.Value.AuthorizationPolicy)
            ?? throw new AuthorizationPolicyNotFoundException(confeatureOptions.Value.AuthorizationPolicy);
        var authenticateResult = await policyEvaluator.AuthenticateAsync(policy, context);
        var authorizationResult = await policyEvaluator.AuthorizeAsync(
            policy,
            authenticateResult,
            context,
            "configuration");

        return authorizationResult.Succeeded;
    }
}
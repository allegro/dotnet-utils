using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

internal class TelemetryContextMiddleware
{
    private readonly RequestDelegate _next;

    public TelemetryContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var current = TelemetryContext.Current;
        context.Items.Add(nameof(TelemetryContext), current);
        await _next(context);
    }
}

internal static class TelemetryContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTelemetryContextMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TelemetryContextMiddleware>();
    }
}

internal class TelemetryContextMiddlewareStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseTelemetryContextMiddleware();
            next(app);
        };
    }
}
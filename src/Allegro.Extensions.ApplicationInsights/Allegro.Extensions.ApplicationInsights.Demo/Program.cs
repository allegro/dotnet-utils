using Allegro.Extensions.ApplicationInsights.AspNetCore;
using Allegro.Extensions.ApplicationInsights.Demo;
using Allegro.Extensions.ApplicationInsights.Prometheus;
using Microsoft.Extensions.Logging;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationInsightsExtensions(builder.Configuration, builder.Environment, b =>
{
    void ConfigureCloudInfo(CustomTelemetryCloudApplicationInfo p)
    {
        p.TeamName = "skyfall";
        p.ApplicationName = "demos-app-insights";
    }

    b.ConfigureTelemetryInitializerLogger(p => p.AddConsole());
    b.AddApplicationInsightsCloudApplicationInfo<CustomTelemetryCloudApplicationInfo>(ConfigureCloudInfo);
    b.AddApplicationInsightsSamplingConfig();
    b.AddApplicationInsightsTelemetryContext();
    b.AddApplicationInsightsFlush();
    b.AddApplicationInsightsSendConfig();
    b.AddApplicationInsightsSamplingExclusions(p => new CustomDependencyForFilter(p), p => new CustomRequestForFilter(p));
}); // customization
builder.Services.AddApplicationInsightsToPrometheus(builder.Configuration); // prometheus

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMetricServer();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
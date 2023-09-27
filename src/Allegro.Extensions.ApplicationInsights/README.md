# Allegro.Extensions.ApplicationInsights

This repo contains a collection of useful Azure Application Insights extensions and utilities, developed as part of [Allegro Pay](https://allegropay.pl/) product which allow you:
- [Allegro.Extensions.ApplicationInsights](#allegroextensionsapplicationinsights)
    - [Better sampling configuration](#better-sampling-configuration)
    - [Sampling exclusions](#sampling-exclusions)
        - [Custom Sampling Filter](#custom-sampling-filter)
        - [ODATA parser](#odata-parser)
        - [Testing ODATA rules](#testing-odata-rules)
    - [TelemetryCloudApplicationInfo](#telemetrycloudapplicationinfo)
    - [TelemetryContext](#telemetrycontext)
    - [ITelemetryInitializerLogger](#itelemetryinitializerlogger)
    - [Flush on shutdown](#flush-on-shutdown)
    - [Disabling sending telemetry on local machine](#disabling-sending-telemetry-on-local-machine)
    - [Startup logger with extensions configuration](#startup-logger-with-extensions-configuration)
    - [Telemetry exporter to prometheus metrics (Allegro.Extensions.ApplicationInsights.Prometheus)](#telemetry-exporter-to-prometheus-metrics-allegroextensionsapplicationinsightsprometheus)
        - [HTTP dependency limitation](#http-dependency-limitation)
    - [Bringing all together](#bringing-all-together)
    - [Demo App](#demo-app)
    - [License](#license)

Go ahead and check out its features below :)

## Better sampling configuration

Normally, when using application insights, if you want to switch between no sampling, fixed sampling, or adaptive sampling you can't easy do that using just appsettings.json. You need to [change](https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling#configuring-adaptive-sampling-for-aspnet-core-applications) your code. The same goes for advanced adaptive sampling configuration.
This feature solves this problem. You can just configure them using appsettings.json

Just add:

```csharp
builder.AddApplicationInsightsSamplingConfig()
```

and now you can configure using such sections (represent default values):

```json
{
    "ApplicationInsights": {
        "SamplingMode": "AdaptiveWithRules",
        "FixedSamplingPercentage": null,
        "SamplingExcludedTypes": "Event;Exception;Trace;PageView",
        "AdaptiveSamplingConfig": null  // it will use default values from Application Insights
    }
}
```

allowed values for sampling mode:

```csharp
public enum SamplingMode
{
    AdaptiveWithRules,
    Adaptive,
    FixedWithRules,
    Fixed,
    Disabled
}
```

AdaptiveSamplingConfig section ([default](https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling#configuring-adaptive-sampling-for-aspnet-applications) values from App Insights)

```json
{
    "AdaptiveSamplingConfig": {
        "MinSamplingPercentage": 0.1,
        "MaxSamplingPercentage": 100.0,
        "MaxTelemetryItemsPerSecond": 5,
        "InitialSamplingPercentage": 100,
        "MovingAverageRatio": 0.25,
        "EvaluationInterval": "00:00:15",
        "SamplingPercentageDecreaseTimeout": "00:02:00",
        "SamplingPercentageIncreaseTimeout": "00:15:00"
    }
}
```

## Sampling exclusions

Application Insights allow your application to work in three sampling modes:
- no sampling at all (all telemetry is sent to the server)
- fixed sampling (some percentage of telemetry is discarded)
- adaptive sampling (more requests application have, more sampling occur)

MS exposed a simple exclusion configuration where you can only tell which telemetry type you want to exclude (`SamplingExcludedTypes`). If you are using sampling, for sure you stumbled upon a need to exclude some telemetries within these categories. It could be all 5xx requests, some unnecessary dependencies, slow requests/dependencies, and so on.

To achieve that, basicly we need to implement ITelemetryInitilizer and implement exclude logic there.

This feature allows you to configure these exclusion using appsettings.json and ODATA semantic.

All you need to enable this:

```csharp
b.AddApplicationInsightsSamplingExclusions();
```

and then:

```json

{
  "ApplicationInsights": {
    "ExcludeFromSamplingTelemetryConfig": {
      "DependencyRules": {
        "AzureServiceBus": "Type eq 'Azure Service Bus' and (duration ge 5000 or success eq false)"
      },
      "RequestRules": {
        "MyService": "Team eq 'skyfall'"
      }
    }
  }
}
```

allowed properties on which you can build ODATA queries,

for Request Telemetry:

```csharp
public record RequestForFilter
{
    public string CloudRoleName { get; }
    public string Name { get; }
    public double Duration { get; }
    public bool? Success { get; }
    public string ResponseCode { get; }
    public string Url { get; }
}
```

for Dependency Telemetry:

```csharp
public record DependencyForFilter
{
    public string CloudRoleName { get; }
    public string Type { get; }
    public string Target { get; }
    public string Name { get; }
    public double Duration { get; }
    public bool? Success { get; }
    public string ResultCode { get; }
}
```

### Custom Sampling Filter

You can inherit from RequestForFilter or DependencyForFilter, extending this way possible properties to query over:

```csharp
public record CustomRequestForFilter : RequestForFilter
{
    public string Team { get; }

    public CustomRequestForFilter(RequestTelemetry requestTelemetry) : base(requestTelemetry)
    {
        Team = requestTelemetry.Properties[nameof(CustomTelemetryCloudApplicationInfo.TeamName)];
    }
}
```

```csharp
public record CustomDependencyForFilter : DependencyForFilter
{
    public string OperationName { get; }
    public string Team { get; }

    public CustomDependencyForFilter(DependencyTelemetry dependencyTelemetry) : base(dependencyTelemetry)
    {
        OperationName = dependencyTelemetry.Context.Operation.Name;
        Team = dependencyTelemetry.Properties[nameof(CustomTelemetryCloudApplicationInfo.TeamName)];
    }
}
```

```csharp
b.AddApplicationInsightsSamplingExclusions(p => new CustomDependencyForFilter(p), p => new CustomRequestForFilter(p));
```

you can combine and leverage TelemetryCloudApplicationInfo or TelemetryContext to enrich additional properties and query over them ;)

### ODATA parser

Library use https://github.com/codecutout/StringToExpression

The implemented ODATA version is 2.0

### Testing ODATA rules

This solution contains tests for "engine", but not configured rules.

But nothing can stop you from writing your own. I encourage you to do so.
How this can be done, you can check out in `Allegro.Extensions.ApplicationInsights.Demo.UnitTests`

## TelemetryCloudApplicationInfo

Application Insights uses the tag `ai.cloud.role` to match telemetry to the specific service/deployment. You can query it in Kusto or see in Application Map relations thanks to it.
That information needs to be passed. You can again, achieve this by implementing ITelemetryInitializer.

This feature does it for you. And a bit more ;)

You can:

```csharp
b.AddApplicationInsightsCloudApplicationInfo()
```

which enrich your every telemetry `ai.cloud.role` with `Environment.GetEnvironmentVariable("ApplicationName")`

or

pass it in code:

```csharp
b.AddApplicationInsightsCloudApplicationInfo(p => p.ApplicationName = "Demo");
```

or make it custom:

```csharp
public class CustomTelemetryCloudApplicationInfo : TelemetryCloudApplicationInfo
{
    /// <summary>
    /// Name of the team assigned to the service
    /// </summary>
    public string? TeamName
    {
        get => this.GetValueOrDefault(nameof(TeamName));
        set
        {
            if (value != null)
            {
                this[nameof(TeamName)] = value;
            }
        }
    }

    public override void LoadFromEnv()
    {
        base.LoadFromEnv();
        Add(nameof(TeamName), Environment.GetEnvironmentVariable(nameof(TeamName)) ?? EmptyPlaceholder);
    }
}
```

```csharp
b.AddApplicationInsightsCloudApplicationInfo<CustomTelemetryCloudApplicationInfo>(
    p =>
    {
        p.ApplicationName = "Demo";
        p.TeamName = "Skyfall";
    });
```

`TelemetryCloudApplicationInfo` is a dictionary, whatever you put there - will be included in your telemetry properties.

## TelemetryContext

Sometimes is nice to pass not only static metadata to your telemetry (what `TelemetryCloudApplicationInfo` is for), but also dynamic, from your request scope.

Again, you can achieve this by writing a custom ITelemetryIntializer which then somehow needs to get that information, and here, things are getting tricky.

For your convenience library provides TelemetryContext which is implemented similar way as HttpContext (asynclocal pattern) and allows you to add data to it, which are scoped to a single request, and they will be automatically included in properties of telemetry related to that request.

To enable this:

```csharp
b.AddApplicationInsightsTelemetryContext();
```

To use:

```csharp
[HttpGet(Name = "GetWeatherForecast")]
public async Task<IEnumerable<WeatherForecast>> Get()
{
    TelemetryContext.Current["TraceIdentifier"] = HttpContext.TraceIdentifier;

    //...
}
```
## ITelemetryInitializerLogger

When you implement your own ITelemetryIntializer, sometimes you need to use ILogger inside it and log some stuff. And when you have application insights configured on your logger - things go crazy, because you end up in StackoverflowException, where your ITelemetryIntializer is calling logger, which has ApplicationInsights configured, so, it calls ITelemetryInitializer again, and the circle goes on...

To avoid that, the library introduces `ITelemetryInitializerLogger` which you can use using DI in your ITelemetryIntializers implementations and is a separatly configured instance of ILogger.

It automatically added when calling `AddApplicationInsightsExtensions`.

You can configure it with additional setup using:

```csharp
b.ConfigureTelemetryInitializerLogger(p => p.AddConsole())
```
## Flush on shutdown

Normally, the SDK sends data typically every 30 sec or whenever the buffer is full (500 items) and no need to manually call Flush() method for web applications except when the application is about to be shut down.

```csharp
b.AddApplicationInsightsFlush()
``` 
resolve just that.

## Disabling sending telemetry on local machine

By default ApplicationInsights sent telemetry to instance of your connection string, even when working on local machine. Sometimes it is not desired. You can put in your appsettings fake instrumentation key and it will work - but then, you need to remember to do that in each service in microservice world. And do not forget to override it on production (otherwise it will also stop sending telemetry)

You can use:

```csharp
b.AddApplicationInsightsSendConfig()
``` 

to disable sending telemetry by default when working on local machine (exactly when `IHostEnvironment.IsDevelopment()`)

You can override it using:

```json
{
  "ApplicationInsights": {
    "DisableLocally": false
  }
}
```

## Startup logger with extensions configuration

Sometimes you need a logger in your startup code, which introduce another chicken or egg problem. To prevent this, Serilog recommends using `.CreateBootstrapLogger()` method. These library expose possibility to apply these Extensions configuration directly to `TelemetryConfiguration` using `ApplyToTelemetryConfiguration` method.

Example:

```csharp
ILogger logger = null; 
services.AddApplicationInsightsExtensions(
    configuration,
    environment,
    builder =>
    {
        logger = new LoggerConfiguration().CreateBootstrapLogger();
        TelemetryConfiguration telemetryConfiguration =
            new TelemetryConfiguration();
        telemetryConfiguration.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
        builder.ApplyToTelemetryConfiguration(telemetryConfiguration);
        logger.WriteTo.ApplicationInsights(
            telemetryConfiguration,
            TelemetryConverter.Traces,
            LogEventLevel.Information)
    });
```

## Telemetry exporter to prometheus metrics (Allegro.Extensions.ApplicationInsights.Prometheus)

In some scenarios, your telemetry volume can be huge. Then you have two choices:
- pay, and retain all telemetry
- or enable sampling and configure it

In the second choice you will lose log-based metrics precision, and depending on how much you are sampling - starts to be useless.

For that purpose, you can enable Prometheus metrics exporter:

```csharp
builder.Services.AddApplicationInsightsToPrometheus(builder.Configuration);
```

which expose two histogram metrics:

`ai_dependency_duration_seconds`

`labels: { "service", "type", "target", "name", "operation_name", "success", "resultCode" }`

and

`ai_request_duration_seconds`

`labels: { "service", "name", "success", "resultCode" }`

you can configure it using `appsettings.json`:

```json
"ApplicationInsights": {
    "ApplicationInsightsToPrometheusMetrics": {
        "DependenciesTypesIncluded": [
            "HTTP"
        ],
        "IncludeBusRequests": false,
        "ShouldGeneralizeHttpDependencyOperationName": false,
        "ShouldGeneralizeHttpDependencyTargetUrl": false,
        "MaxUrisPerHost": 100
    }
}
```

using `DependenciesTypesIncluded` you can narrow dependency telemetry to a given type (for example only HTTP)

using `IncludeBusRequests` you can include request telemetry for Azure Service Bus (it looks for the request telemetry name starting with "Process")

### HTTP dependency limitation

> **Warning**
In high load scenarios and big microservices world, it can produce `ai_dependency_duration_seconds` with huge size, because of `operation_name` and `name ` cardinality labels. `operation_name` consists usually input endpoint (usually with parameter name), and the name is target endpoint URL (usually with parameter values) which leads very quickly to a huge amount of values of these labels.

By default, `operation_name` is empty, and name consist only a METHOD of `target url`.

The library has some protection against it which you can enable and get a proper `operation_name` and target url (`name`)

Firstly, it has a built-in deparametrizator that tries to find query parameter values and change them to constants. So each query path will be recorded as the same label value, despite different param values.
You can enable it with `ShouldGeneralizeHttpDependencyOperationName` or `ShouldGeneralizeHttpDependencyTargetUrl`

Secondly, if it fails and finds more than `MaxUrisPerHost` (100 by default) - it will circuit break for that host and log an error to `IStartupLogger` (you need to enable it)

However, despite these protections, <span style="color:red;font-size:20px">BE CAUCIUS</span>, when using this feature. Check your metrics cardinality and size.

## Bringing all together

All features enabled, with default settings

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationInsightsExtensions(builder.Configuration, out var startupLogger);
builder.Services.AddApplicationInsightsToPrometheus(builder.Configuration);
```

with customization (remember, that method with builder starts from zero, if you do not put anything, nothing will be enabled)

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationInsightsExtensions(builder.Configuration, b =>
{
    void ConfigureCloudInfo(CustomTelemetryCloudApplicationInfo p)
    {
        p.TeamName = "skyfall";
        p.ApplicationName = "demos-app-insights";
    }

    b.AddStartupApplicationInsightsLogger<CustomTelemetryCloudApplicationInfo>(
        out _,
        ConfigureCloudInfo,
        loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        });
    b.AddApplicationInsightsCloudApplicationInfo<CustomTelemetryCloudApplicationInfo>(ConfigureCloudInfo);
    b.AddApplicationInsightsSamplingConfig();
    b.AddApplicationInsightsTelemetryContext();
    b.AddApplicationInsightsSamplingExclusions(p => new CustomDependencyForFilter(p), p => new CustomRequestForFilter(p));
});
builder.Services.AddApplicationInsightsToPrometheus(builder.Configuration);
```

## Demo App

Solution consist a demo app which you can run locally and test all features and API.

## License

Copyright 2023 Allegro Group

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
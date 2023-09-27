using Allegro.Extensions.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.DataContracts;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.ApplicationInsights.Demo;

public record CustomRequestForFilter : RequestForFilter
{
    public string Team { get; }

    public CustomRequestForFilter(RequestTelemetry requestTelemetry) : base(requestTelemetry)
    {
        Team = requestTelemetry.Properties[nameof(CustomTelemetryCloudApplicationInfo.TeamName)];
    }
}

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
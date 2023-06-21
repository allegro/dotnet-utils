# Allegro.Extensions.DependencyCall 

## Problem statement

Purpose of this package is to standardize a way of calling external dependencies and push developers to think about way of dealing with some issues related to each external call.

In most cases developers creates something like:

```c#
public class ExternalService
{
    private readonly HttpClient _httpClient;
    public ExternalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<SomeData> GetData(string id){
        
        HttpResponseMessage response = await _httpClient.GetAsync($"uri/{id}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ResponseData1>();
    }
    
    public async Task SaveData(SomeData data){
        
        string json = JsonConvert.SerializeObject(data); 
        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync($"uri/{id}", httpContent);
        response.EnsureSuccessStatusCode();
        
        return;
    }
}
```

Of course some of developers will additionally solve common network related issues (fallacies of distributed computing) like:
- handle exceptions on serialization/deserialization
- add Polly to handle transient errors
- add logging of kind
- change default timeout to more user friendly

More experienced developers might even add:
- some metrics to be able to observe dependency behaviour
- introduce business fallbacks if possible with monitoring

Last issue related to this approach is assumption, that this service has single responsibility (purpose) to call external service.
From technical point of view, probably we can say so. 
However, from business/application logic perspective each call will have different behaviour for aspects like:
- time to process 
  - can wait longer for save than read data; 
  - depending on business case different timeout might be applied;
- error handling strategy
  - do we support idempotency; 
  - maybe we are able to wait longer and retry in some processes and can't in others;
- fallback strategy 
  - we can fallback GET more likely than POST; 
  - in some cases we can fallback even missing data or outage of database; 
  - depends on business case same api method might be fallback differently;
- usage purpose 
  - some api can expose multiple functionalities for different purposes and mixing them might be hard for user to learn which should be used in his scenario;


## Basic usage

Main part of this packages is `DependencyCall` abstraction. Basic usage is:

```c#
public class SampleDependencyCall : DependencyCall<SampleRequestData, SampleResponseData>
{
    protected override Task<SampleResponseData> Execute(SampleRequestData request, CancellationToken cancellationToken)
    {
        ...
    }

    protected override Task<(ShouldThrowOnError ShouldThrowOnError, SampleResponseData Response)> Fallback(SampleRequestData request, Exception exception, CancellationToken cancellationToken)
    {
        ...
    }
}

public record SampleRequestData(string Data) : IRequest<SampleResponseData>;

public record SampleResponseData(string Data);
```

We need to implement:

- `Execute` - in most cases `httpClient` call to external dependency or any I/O related calls
- `Fallback` - how we should handle errors when they occurs

To use it we need to execute use `IDependencyCallDispatcher.Dispatch` api. 

```c#
public class ApplictionLogic
{
    private readonly IDependencyCallDispatcher _dependencyCallDispatcher;

    public ApplictionLogic(IDependencyCallDispatcher dependencyCallDispatcher)
    {
        _dependencyCallDispatcher = dependencyCallDispatcher;
    }

    public async Task ExecuteLogic(string data)
    {
        var response = await _dependencyCallDispatcher.Dispatch(new SampleRequestData(data));
    }
}
```

We decided to use mediator pattern to be able to:
- automatically register all `DependencyCall` implementations and resolve dependencies with `ServiceProvider`
- separate application logic layer (abstractions like `IDependencyCallDispatcher`, `IRequest` used ) from infrastructure (ex. httpClient, entity framework) 

To register tool you need to:

```c#
services
    .AddDependencyCall(
        applicationAssemblies: assemblies
    )
```

`assemblies` is optional collection of your application code, that will be scanned with `Scrutor` to register all `DependencyCall` instances.
If not provided it will scan code from `AppDomain.CurrentDomain`.

### Advanced usage

`DependencyCall` api delivers some additional concepts that should be considered by developers.

**Call timeout**

We are using `CancellationToken` approach. It is possible to deliver external `CancellationToken` (ex. from api request):

```c#
Task<TResponse> Dispatch<TResponse>(IRequest<TResponse> request, CancellationToken? cancellationToken = null);
```

By default we assume that operations longer than **5 seconds** from user perspective are too long and call will be cancelled after this time.
Of course this cancellation token is passed to `Execute` and `Fallback` method so it is developer decision at the end.

To change default timeout value:

```c#
protected override TimeSpan CancelAfter { get; } = TimeSpan.FromSeconds(10);
```

**Retry policy**

By default we assume that we are not able to deliver any kind of retry policy. It takes to many possibilities and decisions that are known only by developer.

To give possibility to set custom policy we decide to use `Polly` library and expose api in `DependencyCall`:

```c#
protected override IAsyncPolicy<SampleResponseData> CustomPolicy(CancellationToken cancellationToken)
{
    return Policy<SampleResponseData>.Handle<Exception>().FallbackAsync(
        token =>
        {
            return Task.FromResult(new SampleResponseData("Fallback value"));
        });
}
```

## Metrics

Each `DependencyCall` should be threaten as possible risk of failure. That means that we should be able to monitor all issues or fallback usage increase.
For that purpose we deliver `IDependencyCallMetrics` api:

```c#
public interface IDependencyCallMetrics
{
    /// <summary>
    /// Triggered when new dependency call is requested
    /// </summary>
    public void Requested(IRequest request);

    /// <summary>
    /// Triggered when new dependency call was executed successfully
    /// </summary>
    public void Executed(IRequest request);

    /// <summary>
    /// Triggered when new dependency call failed with error
    /// </summary>
    public void Failed(IRequest request, Exception exception);

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback(IRequest request);

    /// <summary>
    /// Used to start timer for histograms. Dispose will be called at the end of call execution
    /// </summary>
    public IDisposable StartTimer(IRequest request);
}
```

By default we deliver `NoOppDependencyCallMetrics` that might be replaced with your own metrics solution with
`DependencyCallBuilder.WithDependencyCallMetrics` api.

### Allegro.Extensions.DependencyCall.Metrics.Prometheus

Prometheus is the mostly used metrics tool in Allegro. For that reason we deliver extension package that will implement `IDependencyCallMetrics` with `prometheus-net` library.

To use it you need only to register it via `DependencyCallBuilder`:

```c#

services
    .AddDependencyCall(
        configureDependencyCall: 
            builder => builder
                .RegisterPrometheusDependencyCallMetrics(applicationName: "app1"))
```
where `applicationName` will be prefix for conventional metrics name, ex:
- app1_dependency_call_duration_metrics_sum
- app1_dependency_call_duration_metrics_count
- app1_dependency_call_duration_metrics_bucket
- app1_dependency_call_used_total{status="requested"}
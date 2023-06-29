# Allegro.Extensions.DependencyCall

## Problem statement

The purpose of this package is to standardize a way of calling external dependencies and push developers to think about ways of dealing with some issues related to each external call.

In most cases, developers create something like:

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

Of course, some developers will additionally solve common network-related issues (fallacies of distributed computing) like:
- handle exceptions on serialization/deserialization
- add Polly to handle transient errors
- add logging of kind
- change default timeout to a more user friendly

In advanced solutions we even might find:
- some metrics to be able to observe dependency behavior
- introduce business fallbacks if possible with monitoring
- recovery strategies (circuit breakers etc.)

The last issue related to this approach is an assumption, that this service has single responsibility (purpose) to call external service.
From a technical point of view, probably we can say so.
However, from a business/application logic perspective, each call will have different behavior for aspects like:
- time to process
    - can wait longer for save than read data;
    - depending on the business case different timeout might be applied;
- error handling strategy
    - do we support idempotency;
    - maybe we can wait longer and retry in some processes and can't in others;
- fallback strategy
    - we can fallback GET more likely than POST;
    - in some cases, we can fallback even if missing data or an outage of a database;
    - depending on the business case same API method might be fallback differently;
- usage purpose
    - some APIs can expose multiple functionalities for different purposes and mixing them might be hard for the user to learn which should be used in his scenario;


## Basic usage

The main part of this package is the `DependencyCall` abstraction. Basic usage is:

```c#
    private class SampleDependencyCall : DependencyCall<SampleRequestData, SampleResponseData>
    {
        protected override Task<SampleResponseData> Execute(
            SampleRequestData request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new SampleResponseData("Data1"));
        }

        protected override Task<FallbackResult> Fallback(
            SampleRequestData request,
            Exception exception,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(FallbackResult.FromValue(new SampleResponseData("Data2")));
        }
    }

    private record SampleRequestData(string Data) : IRequest<SampleResponseData>;

    private record SampleResponseData(string Data);
```

We need to implement:

- `Execute` - in most cases `httpClient` call to external dependency or any I/O-related calls
- `Fallback` - how we should handle errors when they occur

To use it we need to execute `IDependencyCallDispatcher.Dispatch` API.

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

We decided to use a mediator pattern to be able to:
- automatically register all `DependencyCall` implementations and resolve dependencies with `ServiceProvider`
- separate application logic layer (abstractions like `IDependencyCallDispatcher`, and `IRequest` used ) from infrastructure (ex. httpClient, entity framework)
- have a possibility to extend the pipeline in the future with some cross-cutting things like logging (log issues or enrich logs);

To register tool you need to:

```c#
services
    .AddDependencyCall(
        applicationAssemblies: assemblies
    )
```

`assemblies` is an optional collection of your application code base, that will be scanned with `Scrutor` to register all `DependencyCall` instances.
If not provided it will scan code from `AppDomain.CurrentDomain`.

### Advanced usage

`DependencyCall` API delivers some additional concepts that should be considered by developers.

**Call timeout**

We are using the `Polly.TimeoutAsync` with `Pesymistic` strategy approach. 

By default, we assume that operations longer than **5 seconds** from the user perspective are too long and the call will be canceled after this time.

To change the default timeout value:

```c#
protected override TimeSpan CancelAfter => TimeSpan.FromSeconds(10);
```

This value can't be modified in runtime as policy is built only once at first usage.

**Error handling policy**

By default, we assume that we are not able to deliver any kind of error handling policy. It takes too many possibilities and decisions that are known only by developers.

To give the possibility to set custom policy we decide to use the `Polly` library and expose API in `DependencyCall`:

```c#
private class MyClassDependency : DependencyCall<TestRequest, TestResponse>
    {
        private static readonly IAsyncPolicy<TestResponse> SamplePolicy = Policy.NoOpAsync<TestResponse>();
        protected override Task<TestResponse> Execute(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResponse("test data"));
        }

        protected override Task<FallbackResult> Fallback(
            TestRequest request,
            Exception exception,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(FallbackResult.NotSupported);
        }

        protected override IAsyncPolicy<TestResponse> CustomPolicy => SamplePolicy;
    }
```

Policy is cached so not able to change it in runtime.

### Naming conventions

In basic usage each call is composed from 3 files
- `{Name}DependencyCall`
- `{Name}Request`
- `{Name}Resposne`

{Name} should be descriptive and based on business language used in current application logic usage.

Ex. When we need customer address data, we could end with:
- `GetCustomerAddressDependencyCall`
- `GetCustomerAddressRequest`
- `GetCustomerAddressResponse`

We should avoid technical or generic names like `GetDataFromCustomerClientApi` or `GetUserData`. 
Purpose of `DependencyCall` approach is to create component that is based on business need not technical implementation.

## Metrics

Each `DependencyCall` should be threatened as a possible risk of failure. That means that we should be able to monitor all issues or fallback usage increases.
For that purpose, we deliver `IDependencyCallMetrics` API:

```c#
public interface IDependencyCallMetrics
{
    /// <summary>
    /// Triggered when new dependency call was executed successfully
    /// </summary>
    public void Succeeded(IRequest request, TimeSpan duration);

    /// <summary>
    /// Triggered when new dependency call failed with error
    /// </summary>
    public void Failed(IRequest request, Exception exception, TimeSpan duration);

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback(IRequest request, TimeSpan duration);
}
```

By default, we deliver `NoOperationDependencyCallMetrics` that might be replaced with your own metrics solution with
`DependencyCallBuilder.WithDependencyCallMetrics` API.

### Allegro.Extensions.DependencyCall.Metrics.Prometheus

Prometheus is the most used metrics tool in Allegro. For that reason, we deliver an extension package that will implement `IDependencyCallMetrics` with the `prometheus-net` library.

To use it you need only to register it via `DependencyCallBuilder`:

```c#

services
    .AddDependencyCall(
        configureDependencyCall: 
            builder => builder
                .RegisterPrometheusDependencyCallMetrics(applicationName: "app1"))
```
where `applicationName` will be a prefix for conventional metrics name, ex:
- app1_dependency_call_duration_metrics_sum
- app1_dependency_call_duration_metrics_count
- app1_dependency_call_duration_metrics_bucket
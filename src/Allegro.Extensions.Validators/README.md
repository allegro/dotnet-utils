# Allegro.Extensions.Validators

Contains useful classes and extensions validators especially null and non empty checks.

## Fluent validation

This package provides extensions for easy registration of fluent validators.

### Usage

Registering all validators in assembly

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.RegisterFluentValidators(typeof(Startup).Assembly);
}
```

Create IOptions<T> with fluent validation enabled

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddWithFluentValidation<TOptions, TValidator>("configuration section name");
}
```
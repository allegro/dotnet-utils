# Allegro.Extensions.Validators

Contains useful classes and extensions validators especially null and non empty checks.

## Fluent validation

This package provides extensions for easy registration of fluent validators.

### Usage

Create IOptions<T> with fluent validation enabled

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOptionsWithFluentValidation<TOptions, TValidator>("configuration section name");
}
```

Add a fluent validator to IOptionsBuilder<t>

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateFluentValidation();
}
```

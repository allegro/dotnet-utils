# Allegro.Extensions.AspNetCore

This library contains useful extensions and utilities for ASP.NET Core based projects.

## SkipOnProd

This attribute allows to easily disable specific actions or controllers on production environment. 
It uses `Environment` env variable to determine if the action should be enabled.

Remember to configure this feature in `Startup.cs`:

```c#
services
    .AddControllers()
    .AddSkipOnProd(_env); // pass IHostEnvironment _env
```

Then just add the `SkipOnProd` attribute to the controller:
```c#
[ApiController]
[Route("api/test")]
[SkipOnProd]
public class TestController : ControllerBase
```

## ErrorHandling

To standardize way of handling errors (exception or/and request validation) and specially response body to have same format across service/system we introduce extension package to support this requirement.

This extension contain dedicated Error Handling Middleware and custom Invalid Model State Response Factory (for handling request model validation)

### ErrorHandlingMiddleware

To enable this middleware we need to at least configure services in `Startup.cs`:

```c#
services
    .AddAllegroErrorHandlingMiddleware(
        logError: error => ...,
        logWarning: warning => ...
        )
```
and configure application in `Startup.cs`:
```c#
app
    .UseAllegroErrorHandlingMiddleware()
```

To be more independent from logging approach we need to pass action how log should be stored.

This will enable default error handling on all not handled exceptions and produce response with status code 500 in standardize format:

```json
{
  "errors": [
    {
      "code": "UnhandledException",
      "message": "Unhandled Exception",
      "userMessage": null,
      "path": null,
      "details": null
    }
  ]
}
```
Error log action will be executed.


Additionally if `System.ComponentModel.DataAnnotations.ValidationException` is thrown in code middleware will handle it by default with status code 400 and body:

```json
{
  "errors": [
    {
      "code": "ValidationException",
      "message": "Data validation error",
      "userMessage": null,
      "path": null,
      "details": null
    }
  ]
}
```

where `message` is built from validation result if provided. Warning log action will be executed in this case.

#### Handle custom exception with ErrorHandlingConfigurationBuilder

When we need to handle exception in more custom way and control status code and response body we are able to build configuration with `ErrorHandlingConfigurationBuilder`.

```c#
services
    .AddAllegroErrorHandlingMiddleware(
        logError: error => ...,
        logWarning: warning => ...,
        builder => builder.WithCustomHandler(...)...
        )
```

To check possible approaches please refer to examples in Demo app in [ErrorHandlingController](https://github.com/allegro/dotnet-utils/blob/main/src/Allegro.Extensions.AspNetCore/Allegro.Extensions.AspNetCore.Demo/Controllers/ErrorHandlingController.cs).

#### Additional instrumentation with ErrorHandlingConfigurationBuilder

To be able to enrich logs, for example with some context information we are able to register custom instrumentation like:

```c#
services
    .AddAllegroErrorHandlingMiddleware(
        logError: error => ...,
        logWarning: warning => ...,
        builder => builder.WithAdditionalInstrumentation(...)...
        )
```
To check possible approaches please refer to examples in Demo app in [ErrorHandlingController](https://github.com/allegro/dotnet-utils/blob/main/src/Allegro.Extensions.AspNetCore/Allegro.Extensions.AspNetCore.Demo/Controllers/ErrorHandlingController.cs).

### Handling model state error response 

To be able to control response of model state response (Data Annotations attributes in request models) package introduce configurable custom `InvalidModelStateResponseFactory`.

To use it you need to configure services in `Startup.cs`:

```c#
services
    .AddControllers()
    .AddAllegroModelStateValidationHandling();
```

This will enable custom handling of request validation based on data annotations and produces standardize error resposne with status code 400 in format:

```json
{
  "errors": [
    {
      "code": "Invalid",
      "message": "The Id field is required.",
      "userMessage": null,
      "path": null,
      "details": null
    },
    {
      "code": "Invalid",
      "message": "The Id2 field is required.",
      "userMessage": null,
      "path": null,
      "details": null
    }
  ]
}
```

For each model entry error error item will be returned with code and validation message.

### Custom model state error response handling with ErrorHandlingConfigurationBuilder

When for some reason we want to handle response for action in custom way and control response code or body we can configure custom handlign with `ErrorHandlingConfigurationBuilder` in `Startup.cs`:

```c#
services
    .AddControllers()
    .AddAllegroModelStateValidationHandling(builder => builder.WithCustomModelStateValidationErrorHandler(...));
```

To check possible approaches please refer to examples in Demo app in [ErrorHandlingController](https://github.com/allegro/dotnet-utils/blob/main/src/Allegro.Extensions.AspNetCore/Allegro.Extensions.AspNetCore.Demo/Controllers/ErrorHandlingController.cs).
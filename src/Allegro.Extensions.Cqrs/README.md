# Allegro.Extensions.Cqrs

Believe that there is no need to describe it once more and point you to authority:

https://martinfowler.com/bliki/CQRS.html

In this package our custom implementation of tools and markers are delivered.

## Allegro.Extensions.Cqrs.Abstractions

Contains common CQRS set of markers and abstractions like `ICommand`, `IQuery<>`, `ICommandDispatcher`, `IQueryDispatcher`, `ICommandHandler`, `IQueryHandler`.

Additionally we introduce some additional things like [Command and Query Validators](#icommandvalidator-and-iqueryvalidator), or [Fluent Validations](#fluent-validations).

### ICommandValidator and IQueryValidator

In more sophisticated validation cases, that simple DataAnnotations are not enough we introduce `ICommandValidator<ICommand>` and `IQueryValidator<IQuery>` to enables adding some validation logic before command or query execution.

```c#
internal class BarCommandValidator : ICommandValidator<BarCommand>
{
    public Task Validate(BarCommand command)
    {
       // validation logic
    }
}
```

```c#
internal class BarQueryValidator : IQueryValidator<BarQuery>
{
    public Task Validate(BarQuery query, CancellationToken cancellationToken)
    {
       // validation logic
    }
}
```

[Sample](./Allegro.Extensions.Cqrs.Demo/Commands/BarCommand.cs) 

### Fluent validations
You can use `AddCqrsFluentValidations` from package `Allegro.Extensions.Cqrs.FluentValidations` to use [Fluent Validations](https://docs.fluentvalidation.net/en/latest/) instead proposed interfaces.

`services.AddCqrsFluentValidations(cqrsAssemblies)`

```c#
internal class FooCommandFluentValidator : AbstractValidator<FooCommand>
{
    public FooCommandFluentValidator()
    {
        RuleFor(_ => _.Name).NotEmpty();
    }
}
```

[Sample](./Allegro.Extensions.Cqrs.Demo/Commands/BarCommand.cs)

### Decorators

This give opportunity to Decorate handlers with any custom code.
Remember to add `Decorator` attribute to your decorator.  
Thanks to it, it will be excluded from auto-registration of handlers and does not loop ;)

```c#
[Decorator]
internal class BarCommandHandlerDecorator : ICommandHandler<BarCommand>
{
    private readonly ICommandHandler<BarCommand> _decorated;
    private readonly ILogger<BarCommand> _logger;

    public BarCommandHandlerDecorator(ICommandHandler<BarCommand> decorated, ILogger<BarCommand> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task Handle(BarCommand command)
    {
        _logger.LogInformation("Before handle");
        await _decorated.Handle(command);
        _logger.LogInformation("After handle");
    }
}
```
```c#
[Decorator]
internal class BarQueryHandlerDecorator : IQueryHandler<BarQuery, BarData>
{
    private readonly IQueryHandler<BarQuery, BarData> _decorated;
    private readonly ILogger<BarCommand> _logger;

    public BarQueryHandlerDecorator(IQueryHandler<BarQuery, BarData> decorated, ILogger<BarCommand> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task<BarData> Handle(BarQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Before handle");
        var result = await _decorated.Handle(query, cancellationToken);
        _logger.LogInformation("After handle");
        return result;
    }
}
```
Registration with [Scrutor](https://github.com/khellang/Scrutor):
```c#
    services.TryDecorate<ICommandHandler<BarCommand>, BarCommandHandlerDecorator>();
```
```c#
    services.TryDecorate<IQueryHandler<BarQuery, BarData>, BarQueryHandlerDecorator>();
```
Remember to first register all commands handlers and than register custom decorator.
Execution order is reversed from registration order.
(First registered will execute last, last registered - first)

### Samples

Some sample usage could be found:
- [commands](./Allegro.Extensions.Cqrs.Demo/Commands)
- [queries](./Allegro.Extensions.Cqrs.Demo/Queries)

### Allegro.Extensions.Cqrs

This package contains:
- default implementation of `ICommandDispatcher` and `IQueryDispatcher`
- automatic registrations of all `ICommandHandler`, `IQueryHandler`, `ICommandValidator`

For registrations [Scrutor](https://github.com/khellang/Scrutor) packages is used as a tool.

### Why not MediatR?

- for learning purposes
- it is simple code, MediatR is still too much
- better separation of queries and commands for decorators (IPipelineBehavior doesn't allow for this)
- ICommand without return type
- Good article why not: https://cezarypiatek.github.io/post/why-i-dont-use-mediatr-for-cqrs/
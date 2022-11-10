# Allegro.Extensions.Cqrs

Believe that there is no need to describe it once more and point you to authority:

https://martinfowler.com/bliki/CQRS.html

In this package our custom implementation of tools and markers are delivered.

## Allegro.Extensions.Cqrs.Abstractions

Contains common CQRS set of markers and abstractions like `ICommand`, `IQuery<>`, `ICommandDispatcher`, `IQueryDispatcher`, `ICommandHandler`, `IQueryHandler`.

Additionally we introduce some additonaly things like [ICommandValidator](#icommandvalidator), [ICommandExecutionActions](#icommandexecutionactions) and [Decorator](#decoratorattribute).

### ICommandValidator

In more sophisticated validation cases, that simple DataAnnotations are not enough we introduce `ICommandValidator<ICommand>` to enables adding some validation logic before command execution.

```c#
internal class SampleCommandValidator : ICommandValidator<SampleCommand>
{
    public Task Validate(SampleCommand command)
    {
       // validation logic
    }
}
```

[Sample](./Allegro.Extensions.Cqrs.Demo/Controllers/CommandController.cs) 

### ICommandExecutionActions

To be able to run some additional actions before and after command execution (like unit of work, collect metrics, diagnostics etc.) without messing business logic code in handler we introduce `ICommandExecutionActions<>` interface.
We can easily split business logic from some technical noise.

```c#
internal class SampleCommandActions : ICommandExecutionActions<SampleCommand>
{
    public Task Before(SampleCommand command)
    {
       ...
    }

    public Task After(SampleCommand command)
    {
       ...
    }
}
```

[Sample](./Allegro.Extensions.Cqrs.Demo/Controllers/CommandController.cs)

### Decorators

To be able to use decorator pattern we introduced `DecoratorAttribute`. Each handler marked with it wont be registered automatically in services by our extensions and give opportunity to Decorate handlers with any custom code.

Example:
```c#
[Decorator]
internal class CommandHandlerDecorator<T> : ICommandHandler<T> where T : ICommand
{
...
}
```

Registration with [Scrutor](https://github.com/khellang/Scrutor):
```c#
    services.TryDecorate(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
```

Remember to first register all commands handlers and than register custom decorator.

### Samples

Some sample usage could be found:
- [commands](./Allegro.Extensions.Cqrs.Demo/Controllers/CommandController.cs)
- [queries](./Allegro.Extensions.Cqrs.Demo/Controllers/QueryController.cs)

### Allegro.Extensions.Cqrs

This package contains:
- default implementation of `ICommandDispatcher` and `IQueryDispatcher`
- automatic registrations of all `ICommandHandler`, `IQueryHandler`, `ICommandValidator`, `ICommandExecutionActions`

For registrations [Scrutor](https://github.com/khellang/Scrutor) packages is uses as a tool.
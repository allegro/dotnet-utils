using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Commands;

// TODO: can we extract common logic for command and query? Should we?
internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task Send<TCommand>(TCommand command) where TCommand : Command
    {
        // TODO: maybe some configuration to reuse outer scope instead of creating new one
        using var scope = _serviceProvider.CreateScope();

        var commandValidators = scope.ServiceProvider.GetServices<ICommandValidator<TCommand>>();

        await Task.WhenAll(commandValidators.Select(p => p.Validate(command)));

        var handlers = scope.ServiceProvider.GetServices<ICommandHandler<TCommand>>().ToList();

        // TODO: throw this on startup
        if (handlers.Count == 0)
            throw new MissingCommandHandlerException(command);

        if (handlers.Count > 1)
            throw new MultipleCommandHandlerException(command);

        var handler = handlers.Single();

        await handler.Handle(command);
    }
}

internal class MissingCommandHandlerException : Exception
{
    public MissingCommandHandlerException(Command command) : base($"Missing handler for command {command.GetType().FullName}")
    {
    }
}

internal class MultipleCommandHandlerException : Exception
{
    public MultipleCommandHandlerException(Command command) : base($"Multiple handler registration for command {command.GetType().FullName}")
    {
    }
}
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

        var handler = scope.ServiceProvider.GetService<ICommandHandler<TCommand>>();

        if (handler is null)
        {
            // TODO: throw this on startup
            throw new MissingCommandHandlerException(command);
        }

        await handler.Handle(command);
    }
}

internal class MissingCommandHandlerException : Exception
{
    public MissingCommandHandlerException(Command command) : base($"Missing handler for command {command.GetType().FullName}")
    {
    }
}
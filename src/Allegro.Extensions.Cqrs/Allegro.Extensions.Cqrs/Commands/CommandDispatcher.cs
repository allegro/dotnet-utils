using System;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Commands;

internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task Send<TCommand>(TCommand? command) where TCommand : class, ICommand
    {
        if (command is null)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService<ICommandHandler<TCommand>>();

        if (handler is null)
        {
            throw new MissingCommandHandlerException(command);
        }

        await handler.Handle(command);
    }
}

internal class MissingCommandHandlerException : Exception
{
    public MissingCommandHandlerException(ICommand command) : base($"Missing handler for command {command.GetType().FullName}")
    {
    }
}
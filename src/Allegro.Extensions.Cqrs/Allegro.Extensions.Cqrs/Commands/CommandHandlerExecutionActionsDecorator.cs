using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Commands;

namespace Allegro.Extensions.Cqrs.Commands;
// TODO: should we have similar things decorator for queries?
[Decorator]
internal class CommandHandlerExecutionActionsDecorator<T> : ICommandHandler<T> where T : ICommand
{
    private readonly ICommandHandler<T> _handler;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandlerExecutionActionsDecorator(
        ICommandHandler<T> handler,
        IServiceProvider serviceProvider)
    {
        _handler = handler;
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(T command)
    {
        var commandValidator = (ICommandValidator<T>?)_serviceProvider.GetService(typeof(ICommandValidator<T>));

        if (commandValidator is not null)
            await commandValidator.Validate(command);

        var commandExecutionActions =
            ((IEnumerable<ICommandExecutionActions<T>>?)_serviceProvider.GetService(
                typeof(IEnumerable<ICommandExecutionActions<T>>)))?.ToImmutableArray() ??
            ImmutableArray<ICommandExecutionActions<T>>.Empty;

        // TODO: do we need order here?
        if (commandExecutionActions.Any())
            await Task.WhenAll(commandExecutionActions.Select(commandExecutionAction => commandExecutionAction.Before(command)));

        await _handler.Handle(command);

        if (commandExecutionActions.Any())
            await Task.WhenAll(commandExecutionActions.Select(commandExecutionAction => commandExecutionAction.After(command)));
    }
}
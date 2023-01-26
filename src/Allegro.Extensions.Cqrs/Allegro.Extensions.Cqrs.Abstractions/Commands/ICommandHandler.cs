using System.Threading.Tasks;

namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Marker interface to add handler for command. In most cases changes state.
/// </summary>
public interface ICommandHandler<in TCommand> where TCommand : Command
{
    /// <summary>
    /// Handles command request
    /// </summary>
    Task Handle(TCommand command);
}
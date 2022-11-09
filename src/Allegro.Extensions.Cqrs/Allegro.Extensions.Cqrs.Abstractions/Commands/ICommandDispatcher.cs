using System.Threading.Tasks;

namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Cqrs command dispatcher interface
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Sends command to dispatcher
    /// </summary>
    Task Send<TCommand>(TCommand command) where TCommand : class, ICommand;
}
using System.Threading.Tasks;

namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Marker interface to be able to add additional action before/after dispatching command. Actions are not ordered so if multiple actions for one command is implemented they will be executed in random order.
/// </summary>
/// <typeparam name="T">Command for which action should be executed</typeparam>
public interface ICommandExecutionActions<T> where T : class, ICommand
{
    /// <summary>
    /// Action executed before command execution
    /// </summary>
    Task Before(T command);

    /// <summary>
    /// Action executed after command execution
    /// </summary>
    Task After(T command);
}
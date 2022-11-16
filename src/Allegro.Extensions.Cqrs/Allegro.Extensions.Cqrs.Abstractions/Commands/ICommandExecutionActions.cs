using System.Threading.Tasks;

namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Marker interface - adds possibility to inject additional action before/after dispatching command.
/// Be aware that multiple actions defined for same command will be executed in random order.
/// </summary>
/// <typeparam name="T">Command for which action should be executed</typeparam>
public interface ICommandExecutionActions<T> where T : ICommand
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
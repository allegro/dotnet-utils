namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Marker interface that allows to execute more complicated validations before action execution.
/// </summary>
/// <typeparam name="T">Supported command type</typeparam>
public interface ICommandValidator<T> where T : Command
{
    /// <summary>
    /// Validates command before execution. Should throw ValidationException or other exception.
    /// </summary>
    Task Validate(T command);
}
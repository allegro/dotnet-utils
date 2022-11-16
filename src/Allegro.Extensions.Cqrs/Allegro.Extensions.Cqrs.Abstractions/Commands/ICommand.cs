namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Cqrs command marker
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Command id
    /// </summary>
    public string Id { get; } // TODO: strongly typed?
}
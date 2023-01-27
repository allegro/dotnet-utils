using System;

namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Cqrs base representation of command.
/// </summary>
public abstract record Command
{
    /// <summary>
    /// Command identifier
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
}
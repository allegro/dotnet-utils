using System;
using System.Text.Json.Serialization;

namespace Allegro.Extensions.Cqrs.Abstractions.Commands;

/// <summary>
/// Cqrs base representation of command.
/// </summary>
public abstract record Command
{
    /// <summary>
    /// Command identifier
    /// </summary>
    [JsonIgnore]
    public string Id { get; } = Guid.NewGuid().ToString();
}
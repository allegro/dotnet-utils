using System;

namespace Allegro.Extensions.Cqrs.Abstractions.Queries;

/// <summary>
/// Cqrs base representation of query.
/// </summary>
public abstract record Query
{
    /// <summary>
    /// Query identifier
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Cqrs base representation of query.
/// </summary>
public abstract record Query<T> : Query;
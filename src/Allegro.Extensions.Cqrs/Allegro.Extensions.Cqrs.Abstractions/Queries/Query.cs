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
/// <typeparam name="T">Type of data returned by query</typeparam>
public abstract record Query<T> : Query;
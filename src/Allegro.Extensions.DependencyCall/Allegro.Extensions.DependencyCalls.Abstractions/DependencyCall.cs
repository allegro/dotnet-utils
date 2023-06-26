namespace Allegro.Extensions.DependencyCalls.Abstractions;

/// <summary>
/// Abstraction to support any dependency call, that allows to declare some common aspects
/// (ex. fallbacks, metrics, retries, timeout) of any dependency call.
/// </summary>
public abstract class DependencyCall<TRequest>
    where TRequest : Request
{
    /// <summary>
    /// Execute (happy path)
    /// </summary>
    public abstract Task<TExecutedResult> Execute<TExecutedResult>(
        TRequest request,
        CancellationToken cancellationToken)
        where TExecutedResult : Result;

    /// <summary>
    /// Fallback (negative path)
    /// </summary>
    public abstract Task<TFallbackResult> Fallback<TFallbackResult>(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken)
        where TFallbackResult : Result;
}

/// <summary>
/// Dependency call base representation of request.
/// </summary>
public abstract record Request
{
    /// <summary>
    /// Identifier of request
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Dependency call base representation of result.
/// </summary>
public abstract record Result;

// TODO: Create union
// /// <summary>
// /// Success path result
// /// </summary>
// public abstract record ExecutedResult : Result;
//
// /// <summary>
// /// Fallback path result
// /// </summary>
// public abstract record FallbackResult : Result;
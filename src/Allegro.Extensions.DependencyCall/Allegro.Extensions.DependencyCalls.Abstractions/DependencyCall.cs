namespace Allegro.Extensions.DependencyCalls.Abstractions;

/// <summary>
/// Abstraction to support any dependency call, that allows to declare some common aspects
/// (ex. fallbacks, metrics, retries, timeout) of any dependency call.
/// </summary>
public abstract class DependencyCall<TRequest, TResult>
    where TRequest : Request<TResult>
{
    /// <summary>
    /// Execute (happy path)
    /// </summary>
    public abstract Task<TExecutedResult> Execute<TExecutedResult>(
        TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Fallback (negative path)
    /// </summary>
    public abstract Task<TFallbackResult> Fallback<TFallbackResult>(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken);
}

/// <summary>
/// Dependency call base representation of request.
/// </summary>
public abstract record Request<TResult>
{
    /// <summary>
    /// Identifier of request
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
}
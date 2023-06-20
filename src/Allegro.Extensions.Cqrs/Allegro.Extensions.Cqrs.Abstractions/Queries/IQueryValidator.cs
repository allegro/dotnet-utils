namespace Allegro.Extensions.Cqrs.Abstractions.Queries;

/// <summary>
/// Marker interface that allows to execute more complicated validations before action execution.
/// </summary>
/// <typeparam name="T">Supported query type</typeparam>
public interface IQueryValidator<in T> where T : Query
{
    /// <summary>
    /// Validates query before execution. Should throw ValidationException or other exception.
    /// </summary>
    Task Validate(T query, CancellationToken cancellationToken);
}
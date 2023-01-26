using System.Threading;
using System.Threading.Tasks;

namespace Allegro.Extensions.Cqrs.Abstractions.Queries;

/// <summary>
/// Cqrs query dispatcher interface
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Sends query to dispatcher
    /// </summary>
    /// <typeparam name="TResult">Type of data returned by query</typeparam>
    /// <returns>Query data</returns>
    Task<TResult> Query<TResult>(Query<TResult> query, CancellationToken cancellationToken);
}
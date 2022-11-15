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
    /// <typeparam name="TQuery">Type of query</typeparam>
    /// <typeparam name="TResult">Type of data returned by query</typeparam>
    /// <returns>Query data</returns>
    Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>;
}
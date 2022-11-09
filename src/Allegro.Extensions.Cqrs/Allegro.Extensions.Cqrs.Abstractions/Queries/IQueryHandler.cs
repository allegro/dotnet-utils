using System.Threading;
using System.Threading.Tasks;

namespace Allegro.Extensions.Cqrs.Abstractions.Queries;

/// <summary>
/// Cqrs query handler interface
/// </summary>
/// <typeparam name="TQuery">Type of supported query</typeparam>
/// <typeparam name="TResult">Type of data returned by quer</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : class, IQuery<TResult>
{
    /// <summary>
    /// Handles query execution. In most cases reading data from read-model directly.
    /// </summary>
    Task<TResult?> Handle(TQuery query, CancellationToken cancellationToken);
}
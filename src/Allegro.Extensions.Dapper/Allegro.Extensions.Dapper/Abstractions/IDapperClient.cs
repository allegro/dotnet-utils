using System.Data;

namespace Allegro.Extensions.Dapper.Abstractions;

/// <summary>
/// Database client built on top of Dapper.
/// </summary>
public interface IDapperClient
{
    /// <summary>
    /// Execute SQL asynchronously against current database connection.
    /// </summary>
    /// <param name="query">SQL to execute</param>
    /// <param name="param">Parameters to use</param>
    /// <returns>Number of rows affected</returns>
    Task<int> ExecuteAsync(string query, object? param = null);

    /// <summary>
    /// Query asynchronously against current database for a single record.
    /// </summary>
    /// <param name="query">SQL to execute</param>
    /// <param name="param">Parameters to use</param>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <returns><typeparamref name="TResult"/></returns>
    Task<TResult> QuerySingleOrDefaultAsync<TResult>(string query, object? param = null);

    /// <summary>
    /// Query asynchronously against current database for set of records.
    /// </summary>
    /// <param name="query">SQL to execute</param>
    /// <param name="param">Parameters to use</param>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <returns>An enumerable of <typeparamref name="TResult"/></returns>
    Task<IEnumerable<TResult>> QueryAsync<TResult>(string query, object? param = null);

    /// <summary>
    /// Execute SQL queries in a transactions.
    /// If there will be any error encountered, rollback will be performed.
    /// </summary>
    /// <param name="queries">Collection of queries to execute</param>
    /// <param name="isolationLevel">Transaction isolation level</param>
    /// <returns>int</returns>
    public Task ExecuteInTransactionAsync(
        ICollection<(string Query, object Param)> queries,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    /// <summary>
    /// Execute SQL queries in a transactions. Additionally modified rows are returned.
    /// If there will be any error encountered, rollback will be performed.
    /// </summary>
    /// <param name="queries">Collection of queries to execute</param>
    /// <param name="isolationLevel">Transaction isolation level</param>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <returns>An enumerable of <typeparamref name="TResult"/></returns>
    Task<IEnumerable<TResult>> ExecuteAndQueryInTransactionAsync<TResult>(
        ICollection<(string Query, object Param)> queries,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}
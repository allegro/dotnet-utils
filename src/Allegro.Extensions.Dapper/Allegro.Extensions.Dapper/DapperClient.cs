using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Allegro.Extensions.Dapper.Abstractions;
using Dapper;

namespace Allegro.Extensions.Dapper;

/// <summary>
/// Database client which initializes connection to a database,
/// performs operation and makes sure that connection is closed.
/// </summary>
internal class DapperClient : IDapperClient
{
    private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

    public DapperClient(
        IDatabaseConnectionFactory databaseConnectionFactory)
    {
        _databaseConnectionFactory = databaseConnectionFactory;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string query, object? param = null)
    {
        await using var connection = _databaseConnectionFactory.Create();
        await connection.OpenAsync();
        return await connection.ExecuteAsync(query, param);
    }

    /// <inheritdoc />
    public async Task<TResult> QuerySingleOrDefaultAsync<TResult>(string query, object? param = null)
    {
        await using var connection = _databaseConnectionFactory.Create();
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<TResult>(query, param);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(string query, object? param = null)
    {
        await using var connection = _databaseConnectionFactory.Create();
        await connection.OpenAsync();
        return await connection.QueryAsync<TResult>(query, param);
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(
        ICollection<(string Query, object Param)> queries,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        await using var connection = _databaseConnectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync(isolationLevel);

        try
        {
            foreach (var (query, param) in queries)
            {
                await connection.ExecuteAsync(query, param);
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TResult>> ExecuteAndQueryInTransactionAsync<TResult>(
        ICollection<(string Query, object Param)> queries,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        await using var connection = _databaseConnectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync(isolationLevel);

        var results = new List<TResult>(queries.Count);
        try
        {
            foreach (var (query, param) in queries)
            {
                var result = await connection.QuerySingleAsync<TResult>(query, param);
                results.Add(result);
            }

            await transaction.CommitAsync();
            return results;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
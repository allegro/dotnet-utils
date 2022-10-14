using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegro.Extensions.Dapper.Postgres.Exceptions;

namespace Allegro.Extensions.Dapper.Postgres.Abstractions;

/// <summary>
/// Binary copy operation using built-in Postgres mechanism.
/// </summary>
public interface IDapperPostgresBinaryCopyClient
{
    /// <summary>
    /// Binary COPY FROM operation which is data import mechanism to a Postgres table.
    /// </summary>
    /// <param name="tableName">Table name to which data will be copied</param>
    /// <param name="columns">Dictionary of name and type of columns</param>
    /// <param name="selector">Selector of properties to copy from source data type</param>
    /// <param name="source">Source data</param>
    /// <typeparam name="T">Source data type</typeparam>
    /// <returns>Number of affected rows</returns>
    /// <exception cref="InvalidDbConnectionTypeException">Thrown if there is a DbConnection type mismatch
    /// from IDatabaseConnectionFactory</exception>
    public Task<ulong> BinaryCopyAsync<T>(
        string tableName,
        IDictionary<string, DbType> columns,
        Func<T, object[]> selector,
        IAsyncEnumerable<T> source);
}
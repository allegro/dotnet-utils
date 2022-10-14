using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegro.Extensions.Dapper.Postgres.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Exceptions;
using Allegro.Extensions.Dapper.Postgres.Factories;
using Npgsql;
using NpgsqlTypes;
using DbType = Allegro.Extensions.Dapper.Postgres.Abstractions.DbType;

namespace Allegro.Extensions.Dapper.Postgres;

/// <inheritdoc />
internal class DapperPostgresBinaryCopyClient : IDapperPostgresBinaryCopyClient
{
    private readonly PostgresDatabaseConnectionFactory _databaseConnectionFactory;

    public DapperPostgresBinaryCopyClient(
        PostgresDatabaseConnectionFactory databaseConnectionFactory)
    {
        _databaseConnectionFactory = databaseConnectionFactory;
    }

    /// <inheritdoc />
    public async Task<ulong> BinaryCopyAsync<T>(
        string tableName,
        IDictionary<string, DbType> columns,
        Func<T, object[]> selector,
        IAsyncEnumerable<T> source)
    {
        var command = $@"COPY {tableName} ({string.Join(',', columns.Keys)}) FROM STDIN (FORMAT BINARY)";
        var npgsqlColumns = columns.Values
            .Select(GetNpgsqlDbType)
            .ToList();

        await using var connection = _databaseConnectionFactory.Create();
        if (connection is not NpgsqlConnection npgsqlConnection)
        {
            throw new InvalidDbConnectionTypeException(nameof(NpgsqlConnection));
        }

        await npgsqlConnection.OpenAsync();
        await using var writer = await npgsqlConnection.BeginBinaryImportAsync(command);

        await foreach (var sourceRow in source)
        {
            var row = selector(sourceRow);
            await writer.StartRowAsync();
            for (var i = 0; i < columns.Count; i++)
            {
                await writer.WriteAsync(row[i], npgsqlColumns[i]);
            }
        }

        return await writer.CompleteAsync();
    }

    private static NpgsqlDbType GetNpgsqlDbType(DbType dbType)
    {
        return dbType switch
        {
            DbType.Int => NpgsqlDbType.Integer,
            DbType.BigInt => NpgsqlDbType.Bigint,
            DbType.Text => NpgsqlDbType.Text,
            DbType.Date => NpgsqlDbType.Date,
            DbType.Decimal => NpgsqlDbType.Numeric,
            _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, "Unknown type")
        };
    }
}
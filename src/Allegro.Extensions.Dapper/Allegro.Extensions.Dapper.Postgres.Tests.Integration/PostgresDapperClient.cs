using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegro.Extensions.Dapper.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Vabank.Storage.Postgres.Sdk.Tests.Integration.Models;
using Xunit;

namespace Allegro.Extensions.Dapper.Postgres.Tests.Integration;

[Collection("Sequential")]
public class PostgresDapperClient : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory>
{
    private readonly IDapperClient _dapperClient;

    public PostgresDapperClient(
        CustomWebApplicationFactory factory)
    {
        _dapperClient = factory.Services.GetRequiredService<IDapperClient>();
    }

    [Fact(Skip = "Todo - Setup postgress in memory or locally or put connection string in appsetings.Test.json")]
    public async Task PostgresClient_ShouldBeAbleToExecuteQueries()
    {
        await CreateTableIfNotExists();

        await TransactionShouldPerformRollBack();
        await ShouldQueryZeroResults();
        await TransactionShouldCommitAndReturnedModifiedRows();
        await ShouldQueryTwoResults();
        await ShouldFindSingleResult();

        await DropTableIfExists();
    }

    private async Task CreateTableIfNotExists()
    {
        var result = await _dapperClient.ExecuteAsync(SqlQueries.CreateTableIfNotExists);

        result.Should().Be(-1);
    }

    private async Task TransactionShouldPerformRollBack()
    {
        var queries = new List<(string, object)>
        {
            (SqlQueries.InsertRowWithReturning, new { Username = "Roman" }),
            (SqlQueries.InsertRowWithReturning, new { Username = "Roman" })
        };

        var func = async () => await _dapperClient.ExecuteInTransactionAsync(queries);

        await func.Should().ThrowAsync<Exception>();
    }

    private async Task ShouldQueryZeroResults()
    {
        var result = await _dapperClient.QueryAsync<TestModel>(SqlQueries.SelectAllFromTable);

        result.Count().Should().Be(0);
    }

    private async Task TransactionShouldCommitAndReturnedModifiedRows()
    {
        var queries = new List<(string, object)>
        {
            (SqlQueries.InsertRowWithReturning, new { Username = "Pawel" }),
            (SqlQueries.InsertRowWithReturning, new { Username = "Piotr" })
        };

        var result = await _dapperClient.ExecuteAndQueryInTransactionAsync<TestModel>(queries);

        result.Count().Should().Be(2);
    }

    private async Task ShouldQueryTwoResults()
    {
        var result = await _dapperClient.QueryAsync<TestModel>(SqlQueries.SelectAllFromTable);

        result.Count().Should().Be(2);
    }

    private async Task ShouldFindSingleResult()
    {
        var result = await _dapperClient.QuerySingleOrDefaultAsync<TestModel>(SqlQueries.SelectFirstFromTable);

        result.Should().NotBeNull();
        result.Username.Should().Be("Pawel");
    }

    private async Task DropTableIfExists()
    {
        var result = await _dapperClient.ExecuteAsync(SqlQueries.DropTableIfExists);

        result.Should().Be(-1);
    }

    public Task InitializeAsync() =>
        Task.CompletedTask;

    public async Task DisposeAsync() =>
        await DropTableIfExists();
}
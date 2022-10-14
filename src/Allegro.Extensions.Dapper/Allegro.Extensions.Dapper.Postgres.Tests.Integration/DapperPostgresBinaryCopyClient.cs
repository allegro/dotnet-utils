using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegro.Extensions.Dapper.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Abstractions;
using Allegro.Extensions.Dapper.Postgres.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Vabank.Storage.Postgres.Sdk.Tests.Integration.Models;
using Xunit;

namespace Allegro.Extensions.Dapper.Postgres.Tests.Integration;

[Collection("Sequential")]
public class PostgresBinaryCopyClientTests : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PostgresBinaryCopyClientTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "Todo - Setup postgress in memory or locally or put connection string in appsetings.Test.json")]
    public async Task BinaryCopyAsync_ShouldCopyTestData()
    {
        var postgresBinaryCopyClient = _factory.Services.GetRequiredService<IDapperPostgresBinaryCopyClient>();
        var columns = new Dictionary<string, DbType>
        {
            { nameof(TestModel.Username), DbType.Text }
        };
        static object[] Selector(TestModel row) => new object[]
        {
            row.Username!
        };
        var testData = new List<TestModel>
        {
            new(1, "test123456789"),
            new(2, "test234567891")
        }.ToAsyncEnumerable();

        var result = await postgresBinaryCopyClient.BinaryCopyAsync(
            SqlQueries.TableName,
            columns,
            Selector,
            testData);

        result.Should().Be(2);
    }

    public async Task InitializeAsync()
    {
        await ExecuteQuery(SqlQueries.CreateTableIfNotExists);
    }

    public async Task DisposeAsync()
    {
        await ExecuteQuery(SqlQueries.DropTableIfExists);
    }

    private async Task ExecuteQuery(string query)
    {
        var databaseClient = _factory.Services.GetRequiredService<IDapperClient>();

        await databaseClient.ExecuteAsync(query);
    }
}
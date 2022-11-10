using System;
using System.Threading;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Allegro.Extensions.Cqrs.Queries;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedType.Local

namespace Allegro.Extensions.Cqrs.Tests.Unit;

public class QueriesSpec
{
    public class QueryHandlers
    {
        [Fact]
        public async Task Able_to_handle_query()
        {
            var fixture = new Fixture().Build();
            var queryDispatcher = fixture.QueryDispatcher;

            var result = await queryDispatcher.Query(new TestQuery(Guid.NewGuid().ToString()), CancellationToken.None);
            result.Should().Be(new TestData("TestData"));
        }

        [Fact]
        public Task When_handler_is_missing_exception_will_be_thrown()
        {
            var queryDispatcher = new Fixture().Build().QueryDispatcher;
            var act = () => queryDispatcher.Query(new TestQueryNoHandler(Guid.NewGuid().ToString()), CancellationToken.None);

            return act.Should().ThrowAsync<MissingQueryHandlerException>();
        }

        private record TestQuery(string Id) : IQuery<TestData>;

        private record TestData(string Name);

        private class TestQueryHandler : IQueryHandler<TestQuery, TestData>
        {
            public Task<TestData?> Handle(TestQuery query, CancellationToken cancellationToken)
            {
                return Task.FromResult((TestData?)new TestData("TestData"));
            }
        }

        private record TestQueryNoHandler(string Id) : IQuery<TestData>;
    }

    private class Fixture
    {
        private readonly ServiceCollection _serviceCollection;
        private IServiceProvider? _provider;

        public Fixture()
        {
            _serviceCollection = new ServiceCollection();

            _serviceCollection.AddQueries(new[] { typeof(QueriesSpec).Assembly });
        }

        public Fixture Build()
        {
            _provider = _serviceCollection.BuildServiceProvider();
            return this;
        }

        public IQueryDispatcher QueryDispatcher => _provider!.GetRequiredService<IQueryDispatcher>();
    }
}
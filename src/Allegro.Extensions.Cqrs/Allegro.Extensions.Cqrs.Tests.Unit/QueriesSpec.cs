using System.ComponentModel.DataAnnotations;
using Allegro.Extensions.Cqrs.Abstractions;
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

            var result = await queryDispatcher.Query(new TestQuery(), CancellationToken.None);
            result.Should().Be(new TestData("TestData"));
        }

        [Fact]
        public Task When_handler_is_missing_exception_will_be_thrown()
        {
            var queryDispatcher = new Fixture().Build().QueryDispatcher;
            var act = () => queryDispatcher.Query(new TestQueryNoHandler(), CancellationToken.None);

            return act.Should().ThrowAsync<MissingQueryHandlerException>();
        }

        [Fact]
        public Task When_multiple_handlers_registered_exception_thrown()
        {
            var queryDispatcher = new Fixture().Build().QueryDispatcher;
            var act = () => queryDispatcher.Query(new MultipleHandlerQueryTest(), CancellationToken.None);

            return act.Should().ThrowAsync<MultipleQueryHandlerException<TestData>>();
        }

        private sealed record MultipleHandlerQueryTest : Query<TestData>;

        private sealed class FirstQueryHandler : IQueryHandler<MultipleHandlerQueryTest, TestData>
        {
            public Task<TestData> Handle(MultipleHandlerQueryTest query, CancellationToken cancellationToken)
            {
                return Task.FromResult(new TestData("TestData"));
            }
        }

        private sealed class SecondQueryHandler : IQueryHandler<MultipleHandlerQueryTest, TestData>
        {
            public Task<TestData> Handle(MultipleHandlerQueryTest query, CancellationToken cancellationToken)
            {
                return Task.FromResult(new TestData("TestData"));
            }
        }

        private sealed record TestQuery : Query<TestData>;

        private sealed record TestData(string Name);

        private sealed class TestQueryHandler : IQueryHandler<TestQuery, TestData>
        {
            public Task<TestData> Handle(TestQuery query, CancellationToken cancellationToken)
            {
                return Task.FromResult(new TestData("TestData"));
            }
        }

        private sealed record TestQueryNoHandler : Query<TestData>;
    }

    public class QueryValidator
    {
        [Fact]
        public Task Able_to_execute_query_validator()
        {
            var queryDispatcher = new Fixture().Build().QueryDispatcher;
            var act = () => queryDispatcher.Query(new NotValidTestQuery(), CancellationToken.None);

            return act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Query_validator_executed_before_query_decorator()
        {
            var fixture = new Fixture().AddDecorator<IQueryHandler<NotValidTestQuery, int>, TestQueryHandlerDecorator>()
                .Build();
            var commandDispatcher = fixture.QueryDispatcher;
            var act = () => commandDispatcher.Query(new NotValidTestQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();

            fixture.VerifyQueryDecoratorsWereNotExecuted();
        }

        private sealed record NotValidTestQuery : Query<int>;

        private sealed class TestQueryValidator : IQueryValidator<NotValidTestQuery>
        {
            public Task Validate(NotValidTestQuery query, CancellationToken cancellationToken)
            {
                throw new ValidationException("Test query validation exception");
            }
        }

        private sealed class TestQueryHandler : IQueryHandler<NotValidTestQuery, int>
        {
            public Task<int> Handle(NotValidTestQuery query, CancellationToken cancellationToken)
            {
                return Task.FromResult(1);
            }
        }

        [Decorator]
        private sealed class TestQueryHandlerDecorator : IQueryHandler<NotValidTestQuery, int>
        {
            private readonly IQueryHandler<NotValidTestQuery, int> _handler;
            private readonly QueryLog _queryLog;

            public TestQueryHandlerDecorator(IQueryHandler<NotValidTestQuery, int> handler, QueryLog queryLog)
            {
                _handler = handler;
                _queryLog = queryLog;
            }

            public async Task<int> Handle(NotValidTestQuery query, CancellationToken cancellationToken)
            {
                _queryLog.ExecutedQueriesLog.Add($"Before {query}");
                var result = await _handler.Handle(query, cancellationToken);
                _queryLog.ExecutedQueriesLog.Add($"After {query}");
                return result;
            }
        }
    }

    public class CommandDecorators
    {
        [Fact]
        public async Task Able_to_execute_query_decorators()
        {
            var fixture = new Fixture().AddDecorator<IQueryHandler<TestQuery, int>, TestQueryHandlerDecorator>()
                .Build();
            var queryDispatcher = fixture.QueryDispatcher;
            var query = new TestQuery();
            await queryDispatcher.Query(query, CancellationToken.None);
            fixture.VerifyQueryWithDecoratorWasHandled(query);
        }

        private sealed record TestQuery : Query<int>;

        private sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
        {
            private readonly QueryLog _queryLog;

            public TestQueryHandler(QueryLog queryLog)
            {
                _queryLog = queryLog;
            }

            public Task<int> Handle(TestQuery query, CancellationToken cancellationToken)
            {
                _queryLog.ExecutedQueriesLog.Add(query.ToString());
                return Task.FromResult(1);
            }
        }

        [Decorator]
        private sealed class TestQueryHandlerDecorator : IQueryHandler<TestQuery, int>
        {
            private readonly IQueryHandler<TestQuery, int> _handler;
            private readonly QueryLog _queryLog;

            public TestQueryHandlerDecorator(IQueryHandler<TestQuery, int> handler, QueryLog queryLog)
            {
                _handler = handler;
                _queryLog = queryLog;
            }

            public async Task<int> Handle(TestQuery query, CancellationToken cancellationToken)
            {
                _queryLog.ExecutedQueriesLog.Add($"Before {query}");
                var result = await _handler.Handle(query, cancellationToken);
                _queryLog.ExecutedQueriesLog.Add($"After {query}");
                return result;
            }
        }
    }

    private sealed class Fixture
    {
        private readonly ServiceCollection _serviceCollection;
        private IServiceProvider? _provider;

        public Fixture()
        {
            _serviceCollection = new ServiceCollection();

            _serviceCollection.AddQueries(new[] { typeof(QueriesSpec).Assembly });
            _serviceCollection.AddSingleton<QueryLog>();
        }

        public Fixture AddDecorator<TType, TDecorator>()
            where TDecorator : TType
        {
            _serviceCollection.TryDecorate<TType, TDecorator>();
            return this;
        }

        public Fixture Build()
        {
            _provider = _serviceCollection.BuildServiceProvider();
            return this;
        }

        public IQueryDispatcher QueryDispatcher => _provider!.GetRequiredService<IQueryDispatcher>();

        public void VerifyQueryWithDecoratorWasHandled(Query testQuery)
        {
            var expectedLogs = new List<string>()
            {
                $"Before {testQuery}", testQuery.ToString()!, $"After {testQuery}",
            };

            var storage = _provider!.GetRequiredService<QueryLog>();
            storage.ExecutedQueriesLog.Should().BeEquivalentTo(expectedLogs);
        }

        public void VerifyQueryDecoratorsWereNotExecuted()
        {
            var storage = _provider!.GetRequiredService<QueryLog>();
            storage.ExecutedQueriesLog.Should().BeEmpty();
        }
    }

    private sealed class QueryLog
    {
        public List<string> ExecutedQueriesLog { get; } = new();
    }
}
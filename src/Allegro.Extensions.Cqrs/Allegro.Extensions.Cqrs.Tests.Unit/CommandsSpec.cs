using System.ComponentModel.DataAnnotations;
using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Allegro.Extensions.Cqrs.Commands;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedType.Local

namespace Allegro.Extensions.Cqrs.Tests.Unit;

public class CommandsSpec
{
    public class CommandHandlers
    {
        [Fact]
        public async Task Able_to_handle_command()
        {
            var fixture = new Fixture().Build();
            var commandDispatcher = fixture.CommandDispatcher;

            var testCommand = new TestCommand();
            await commandDispatcher.Send(testCommand);
            fixture.VerifyCommandWasHandled(testCommand);
        }

        [Fact]
        public Task When_handler_is_missing_exception_will_be_thrown()
        {
            var commandDispatcher = new Fixture().Build().CommandDispatcher;
            var act = () => commandDispatcher.Send(new TestCommandNoHandler());

            return act.Should().ThrowAsync<MissingCommandHandlerException>();
        }

        [Fact]
        public Task When_multiple_handlers_registered_exception_thrown()
        {
            var queryDispatcher = new Fixture().Build().CommandDispatcher;
            var act = () => queryDispatcher.Send(new MultipleHandlerCommandTest());

            return act.Should().ThrowAsync<MultipleCommandHandlerException>();
        }

        private record MultipleHandlerCommandTest : Command;

        private class FirstCommandHandler : ICommandHandler<MultipleHandlerCommandTest>
        {
            private readonly CommandLog _commandLog;

            public FirstCommandHandler(CommandLog commandLog)
            {
                _commandLog = commandLog;
            }

            public Task Handle(MultipleHandlerCommandTest command)
            {
                _commandLog.ExecutedCommandsLog.Add(command.ToString());
                return Task.CompletedTask;
            }
        }

        private class SecondCommandHandler : ICommandHandler<MultipleHandlerCommandTest>
        {
            private readonly CommandLog _commandLog;

            public SecondCommandHandler(CommandLog commandLog)
            {
                _commandLog = commandLog;
            }

            public Task Handle(MultipleHandlerCommandTest command)
            {
                _commandLog.ExecutedCommandsLog.Add(command.ToString());
                return Task.CompletedTask;
            }
        }

        private record TestCommand : Command;

        private class TestCommandHandler : ICommandHandler<TestCommand>
        {
            private readonly CommandLog _log;

            public TestCommandHandler(CommandLog log)
            {
                _log = log;
            }

            public Task Handle(TestCommand command)
            {
                _log.ExecutedCommandsLog.Add(command.ToString());
                return Task.CompletedTask;
            }
        }

        private record TestCommandNoHandler : Command;
    }

    public class CommandValidator
    {
        [Fact]
        public Task Able_to_execute_command_validator()
        {
            var commandDispatcher = new Fixture().Build().CommandDispatcher;
            var act = () => commandDispatcher.Send(new NotValidTestCommand());

            return act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Command_validator_executed_before_command_action()
        {
            var fixture = new Fixture().AddDecorator<ICommandHandler<NotValidTestCommand>, TestCommandHandlerDecorator>()
                .Build();
            var commandDispatcher = fixture.CommandDispatcher;
            var act = () => commandDispatcher.Send(new NotValidTestCommand());

            await act.Should().ThrowAsync<ValidationException>();

            fixture.VerifyCommandActionsWereNotExecuted();
        }

        private record NotValidTestCommand : Command;

        private class TestCommandValidator : ICommandValidator<NotValidTestCommand>
        {
            public Task Validate(NotValidTestCommand command)
            {
                throw new ValidationException("Test command validation exception");
            }
        }

        private class TestCommandHandler : ICommandHandler<NotValidTestCommand>
        {
            public Task Handle(NotValidTestCommand command)
            {
                return Task.CompletedTask;
            }
        }

        [Decorator]
        private class TestCommandHandlerDecorator : ICommandHandler<NotValidTestCommand>
        {
            private readonly ICommandHandler<NotValidTestCommand> _handler;
            private readonly CommandLog _commandLog;

            public TestCommandHandlerDecorator(ICommandHandler<NotValidTestCommand> handler, CommandLog commandLog)
            {
                _handler = handler;
                _commandLog = commandLog;
            }

            public async Task Handle(NotValidTestCommand command)
            {
                _commandLog.ExecutedCommandsLog.Add($"Before {command.ToString()}");
                await _handler.Handle(command);
                _commandLog.ExecutedCommandsLog.Add($"After {command.ToString()}");
            }
        }
    }

    public class CommandDecorators
    {
        [Fact]
        public async Task Able_to_execute_command_actions()
        {
            var fixture = new Fixture().AddDecorator<ICommandHandler<TestCommand>, TestCommandHandlerDecorator>()
                .Build();
            var commandDispatcher = fixture.CommandDispatcher;
            var command = new TestCommand();
            await commandDispatcher.Send(command);
            fixture.VerifyCommandWithActionsWasHandled(command);
        }

        private record TestCommand : Command;

        private class TestCommandHandler : ICommandHandler<TestCommand>
        {
            private readonly CommandLog _commandLog;

            public TestCommandHandler(CommandLog commandLog)
            {
                _commandLog = commandLog;
            }

            public Task Handle(TestCommand command)
            {
                _commandLog.ExecutedCommandsLog.Add(command.ToString());
                return Task.CompletedTask;
            }
        }

        [Decorator]
        private class TestCommandHandlerDecorator : ICommandHandler<TestCommand>
        {
            private readonly ICommandHandler<TestCommand> _handler;
            private readonly CommandLog _commandLog;

            public TestCommandHandlerDecorator(ICommandHandler<TestCommand> handler, CommandLog commandLog)
            {
                _handler = handler;
                _commandLog = commandLog;
            }

            public async Task Handle(TestCommand command)
            {
                _commandLog.ExecutedCommandsLog.Add($"Before {command.ToString()}");
                await _handler.Handle(command);
                _commandLog.ExecutedCommandsLog.Add($"After {command.ToString()}");
            }
        }
    }

    private class Fixture
    {
        private readonly ServiceCollection _serviceCollection;
        private IServiceProvider? _provider;

        public Fixture()
        {
            _serviceCollection = new ServiceCollection();

            _serviceCollection.AddCommands(new[] { typeof(CommandsSpec).Assembly });

            _serviceCollection.AddSingleton<CommandLog>();
        }

        public Fixture Build()
        {
            _provider = _serviceCollection.BuildServiceProvider();
            return this;
        }

        public ICommandDispatcher CommandDispatcher => _provider!.GetRequiredService<ICommandDispatcher>();

        public Fixture AddDecorator<TType, TDecorator>()
            where TDecorator : TType
        {
            _serviceCollection.TryDecorate<TType, TDecorator>();
            return this;
        }

        public void VerifyCommandWasHandled(Command testCommand)
        {
            var storage = _provider!.GetRequiredService<CommandLog>();

            storage.ExecutedCommandsLog.Single().Should().Be(testCommand.ToString());
        }

        public void VerifyCommandWithActionsWasHandled(Command testCommand)
        {
            var expectedLogs = new List<string>()
            {
                $"Before {testCommand.ToString()}", testCommand.ToString()!, $"After {testCommand.ToString()}",
            };

            var storage = _provider!.GetRequiredService<CommandLog>();
            storage.ExecutedCommandsLog.Should().BeEquivalentTo(expectedLogs);
        }

        public void VerifyCommandActionsWereNotExecuted()
        {
            var storage = _provider!.GetRequiredService<CommandLog>();
            storage.ExecutedCommandsLog.Should().BeEmpty();
        }
    }

    private class CommandLog
    {
        public List<string> ExecutedCommandsLog { get; } = new();
    }
}
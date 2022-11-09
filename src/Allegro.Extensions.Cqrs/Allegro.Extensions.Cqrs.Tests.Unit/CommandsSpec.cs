using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task Able_to_register_custom_command_with_handler()
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

        private record TestCommand : ICommand
        {
            public string Id { get; } = Guid.NewGuid().ToString();
        }

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

        private record TestCommandNoHandler : ICommand
        {
            public string Id { get; } = Guid.NewGuid().ToString();
        }
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

        private record NotValidTestCommand : ICommand
        {
            public string Id { get; } = Guid.NewGuid().ToString();
        }

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
    }

    public class CommandActions
    {
        [Fact]
        public async Task Able_to_execute_command_actions()
        {
            var fixture = new Fixture().Build();
            var commandDispatcher = fixture.CommandDispatcher;
            var command = new TestCommand();
            await commandDispatcher.Send(command);
            fixture.VerifyCommandWithActionsWasHandled(command);
        }

        private record TestCommand : ICommand
        {
            public string Id { get; } = Guid.NewGuid().ToString();
        }

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

        private class TestCommandHandlerActions : ICommandExecutionActions<TestCommand>
        {
            private readonly CommandLog _commandLog;

            public TestCommandHandlerActions(CommandLog commandLog)
            {
                _commandLog = commandLog;
            }

            public Task Before(TestCommand command)
            {
                _commandLog.ExecutedCommandsLog.Add($"Before {command.ToString()}");
                return Task.CompletedTask;
            }

            public Task After(TestCommand command)
            {
                _commandLog.ExecutedCommandsLog.Add($"After {command.ToString()}");
                return Task.CompletedTask;
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

        public void VerifyCommandWasHandled(ICommand testCommand)
        {
            var storage = _provider!.GetRequiredService<CommandLog>();

            storage.ExecutedCommandsLog.Single().Should().Be(testCommand.ToString());
        }

        public void VerifyCommandWithActionsWasHandled(ICommand testCommand)
        {
            var expectedLogs = new List<string>()
            {
                $"Before {testCommand.ToString()}", testCommand.ToString()!, $"After {testCommand.ToString()}",
            };

            var storage = _provider!.GetRequiredService<CommandLog>();
            storage.ExecutedCommandsLog.Should().BeEquivalentTo(expectedLogs);
        }
    }

    private class CommandLog
    {
        public List<string> ExecutedCommandsLog { get; } = new();
    }
}
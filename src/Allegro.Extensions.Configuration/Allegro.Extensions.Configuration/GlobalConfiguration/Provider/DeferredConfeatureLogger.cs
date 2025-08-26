using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.Configuration.GlobalConfiguration.Provider;

/// <summary>
/// <see cref="ILogger"/> implementation that collects all logger invocations to be executed later,
/// on an actual logger instance. It prints the log events immediately on the console.
/// </summary>
internal class DeferredConfeatureLogger : ILogger
{
    private readonly List<Action<ILogger>> _deferred = new();
    public IReadOnlyList<Action<ILogger>> Deferred => _deferred.AsReadOnly();

    public IDisposable BeginScope<TState>(TState state)
    {
#pragma warning disable MA0025
        throw new NotImplementedException();
#pragma warning restore MA0025
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine(formatter(state, exception));

        if (exception != null)
        {
            Console.WriteLine(exception);
        }

        _deferred.Add(logger => logger.Log(logLevel, eventId, state, exception, formatter));
    }
}
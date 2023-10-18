using Serilog;
using Serilog.Events;

namespace Allegro.Extensions.Configuration.GlobalConfiguration.Provider;

/// <summary>
/// <see cref="ILogger"/> implementation that collects all logger invocations to be executed later,
/// on an actual logger instance. It prints the log events immediately on the console.
/// </summary>
internal class DeferredConfeatureLogger : ILogger
{
    private readonly List<Action<ILogger>> _deferred = new();
    public IReadOnlyList<Action<ILogger>> Deferred => _deferred.AsReadOnly();

    public void Write(LogEvent logEvent)
    {
        var stringWriter = new StringWriter();
        logEvent.RenderMessage(stringWriter);
        Console.WriteLine(stringWriter.ToString());

        if (logEvent.Exception != null)
        {
            Console.WriteLine(logEvent.Exception);
        }

        _deferred.Add(logger => logger.Write(logEvent));
    }
}
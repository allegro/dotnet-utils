using System.Collections.Generic;
using System.Linq;
using Allegro.Extensions.AspNetCore.ErrorHandling.Internals;

// ReSharper disable ClassNeverInstantiated.Global

namespace Allegro.Extensions.AspNetCore.ErrorHandling;

public record Error
{
    private readonly ICollection<ErrorData> _errors;

    public LogLevel LogLevel { get; }

    public int ResponseCode { get; }

    public IEnumerable<ErrorData> Errors => _errors;

    public object? CustomResponse { get; private set; }

    private Error(LogLevel logLevel, int responseCode)
    {
        LogLevel = logLevel;
        ResponseCode = responseCode;
        _errors = new List<ErrorData>();
    }

    public static Error Create(LogLevel logLevel, int responseCode) => new(logLevel, responseCode);
    public Error WithCustomResponse(object customResponse)
    {
        CustomResponse = customResponse;
        return this;
    }

    public Error AddErrorData(
        string code,
        string message,
        string userMessage,
        string? path = null,
        string? details = null)
    {
        _errors.Add(new ErrorData(code, message, userMessage, path, details));
        return this;
    }

    internal object BuildErrorResponse()
    {
        return
            CustomResponse ?? new ErrorResponsesHolder(
                Errors?
                    .Select(x => new ErrorResponse(x.Code, x.Message, x.UserMessage, x.Path, x.Details))
                    .ToList() ?? Enumerable.Empty<ErrorResponse>());
    }
}

public record ErrorData(
    string Code,
    string Message,
    string? UserMessage = default,
    string? Path = default,
    string? Details = default);

public enum LogLevel
{
    NoLog = 0,
    Warning = 1,
    Error = 2
}
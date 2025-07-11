// ReSharper disable ClassNeverInstantiated.Global

namespace Allegro.Extensions.AspNetCore.ErrorHandling;

/// <summary>
/// Error definition
/// </summary>
public record Error
{
    private readonly ICollection<ErrorData> _errors;

    /// <summary>
    /// Log level of error
    /// </summary>
    public LogLevel LogLevel { get; }

    /// <summary>
    /// Http response code
    /// </summary>
    public int ResponseCode { get; }

    /// <summary>
    /// Collection of error data related to error
    /// </summary>
    public IEnumerable<ErrorData> Errors => _errors;

    /// <summary>
    /// Custom response structure to support legacy error responses
    /// </summary>
    public object? CustomResponse { get; private set; }

    private Error(LogLevel logLevel, int responseCode)
    {
        LogLevel = logLevel;
        ResponseCode = responseCode;
        _errors = new List<ErrorData>();
    }

    /// <summary>
    /// Factory method
    /// </summary>
    public static Error Create(LogLevel logLevel, int responseCode) => new(logLevel, responseCode);

    /// <summary>
    /// Builder method to define custom response object for legacy services
    /// </summary>
    public Error WithCustomResponse(object customResponse)
    {
        CustomResponse = customResponse;
        return this;
    }

    /// <summary>
    /// Adds error data related to error
    /// </summary>
    public Error AddErrorData(
        string code,
        string message,
        string? userMessage = null,
        string? path = null,
        string? details = null)
    {
        _errors.Add(new ErrorData(code, message, userMessage, path, details));
        return this;
    }

    internal object BuildErrorResponse()
    {
        return
            CustomResponse ?? new ErrorResponse(
                Errors?
                    .Select(x => new ErrorData(x.Code, x.Message, x.UserMessage, x.Path, x.Details))
                    .ToList() ?? Enumerable.Empty<ErrorData>());
    }
}

/// <summary>
/// Default error response contract
/// </summary>
public record ErrorResponse(
    IEnumerable<ErrorData> Errors);

/// <summary>
/// Data related to error
/// </summary>
/// <param name="Code">Custom code</param>
/// <param name="Message">System message</param>
/// <param name="UserMessage">User friendly message</param>
/// <param name="Path">Url or custom path to error</param>
/// <param name="Details">Additional message</param>
public record ErrorData(
    string Code,
    string Message,
    string? UserMessage = default,
    string? Path = default,
    string? Details = default);

/// <summary>
/// Log levels
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Log wont be stored
    /// </summary>
    NoLog = 0,

    /// <summary>
    /// Warning level log
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error level log
    /// </summary>
    Error = 2
}
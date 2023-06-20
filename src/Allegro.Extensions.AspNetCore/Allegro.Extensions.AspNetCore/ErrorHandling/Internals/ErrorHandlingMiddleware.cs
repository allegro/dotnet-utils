using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Allegro.Extensions.AspNetCore.ErrorHandling.Internals;

internal class ErrorHandlingMiddleware : IMiddleware
{
    private readonly IDictionary<Type, Func<Exception, Error>> _customErrorHandlerMap;
    private readonly ICollection<Func<HttpContext, IDisposable>> _additionalInstrumentation;
    private readonly IErrorSerializer _errorSerializer;

    private readonly Action<(string Message, Exception Exception)> _logError;
    private readonly Action<(string Message, Exception Exception)> _logWarning;

    public ErrorHandlingMiddleware(
        IDictionary<Type, Func<Exception, Error>> customErrorHandlerMap,
        ICollection<Func<HttpContext, IDisposable>> additionalInstrumentation,
        IErrorSerializer errorSerializer,
        Action<(string Message, Exception Exception)> logError,
        Action<(string Message, Exception Exception)> logWarning)
    {
        _customErrorHandlerMap = customErrorHandlerMap;
        _additionalInstrumentation = additionalInstrumentation;
        _errorSerializer = errorSerializer;
        _logError = logError;
        _logWarning = logWarning;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            var additionalInstrumentationToDispose = _additionalInstrumentation.Count > 0
                ? _additionalInstrumentation.Select(ai => ai.Invoke(context))
                : null;
            try
            {
                var customHandler = TryGetCustomHandler(ex);
                var (responseStatusCode, errorResponse) = ex switch
                {
                    { } cex when customHandler is not null
                        => HandleCustomException(customHandler(cex), cex),
                    ValidationException vex => HandleValidationException(vex),
                    _ => HandleDefaultException(ex)
                };

                if (context.Response.HasStarted)
                {
                    _logError(("Response has already started, not able to handle it.", ex));
                    // in some cases we might not be able o wrote to response, so whe just log this fact and rethrow
                    throw;
                }

                var responseBody = _errorSerializer.Serialize(errorResponse);
                context.Response.StatusCode = responseStatusCode;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseBody);
                await context.Response.WriteAsync(responseBody, Encoding.UTF8);
            }
            finally
            {
                if (additionalInstrumentationToDispose != null)
                {
                    foreach (var disposable in additionalInstrumentationToDispose)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }

    private Func<Exception, Error>? TryGetCustomHandler(Exception cex)
    {
        var exType = cex.GetType();
        var customHandlerKey = _customErrorHandlerMap.Keys.FirstOrDefault(type => type == exType || exType.IsSubclassOf(type));

        return customHandlerKey is null ? null : _customErrorHandlerMap[customHandlerKey];
    }

    private (int ResponseCode, object ErrorResponsesHolder) HandleCustomException(Error error, Exception ex)
    {
        switch (error.LogLevel)
        {
            case LogLevel.Error:
                _logError((ex.Message, ex));
                break;
            case LogLevel.Warning:
                _logWarning((ex.Message, ex));
                break;
            case LogLevel.NoLog:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(error.LogLevel), error.LogLevel, "Unknown log level");
        }

        return (error.ResponseCode, error.BuildErrorResponse());
    }

    private (int ResponseCode, object ErrorResponsesHolder) HandleValidationException(ValidationException ex)
    {
        _logWarning((ex.Message, ex));

        return (StatusCodes.Status400BadRequest, new ErrorResponsesHolder(
            Errors: new[]
            {
                new ErrorResponse(
                    nameof(ValidationException),
                    BuildValidationResultErrorMessage(ex))
            }));
    }

    private static string BuildValidationResultErrorMessage(ValidationException ex) =>
        ex.ValidationResult.ErrorMessage ??
        $"Validation error on fields: {string.Join(", ", ex.ValidationResult.MemberNames)}";

    private (int ResponseCode, object ErrorResponsesHolder) HandleDefaultException(Exception ex)
    {
        _logError((ex.Message, ex));
        return (StatusCodes.Status500InternalServerError, new ErrorResponsesHolder(
            Errors: new[]
            {
                new ErrorResponse(
                    "UnhandledException",
                    "Unhandled Exception")
            }));
    }
}
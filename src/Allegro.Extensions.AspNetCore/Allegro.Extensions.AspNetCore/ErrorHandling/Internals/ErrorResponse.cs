namespace Allegro.Extensions.AspNetCore.ErrorHandling.Internals;

internal record ErrorResponse(
    string Code,
    string Message,
    string? UserMessage = default,
    string? Path = default,
    string? Details = default);

internal record ErrorResponsesHolder(
    IEnumerable<ErrorResponse> Errors);
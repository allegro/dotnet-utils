using System.ComponentModel.DataAnnotations;
using System.Net;
using Allegro.Extensions.AspNetCore.ErrorHandling;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.AspNetCore.Demo.Controllers;

[ApiController]
[Route("api/errorhandling")]
public class ErrorHandlingController : ControllerBase
{
    [HttpGet("default")]
    public IActionResult Default()
    {
#pragma warning disable CA2201
        throw new Exception("Default error handling for not mapped exception");
#pragma warning restore CA2201
    }

    [HttpGet("validationException")]
    public IActionResult ValidationException()
    {
        throw new ValidationException(
            new ValidationResult(
                errorMessage: "Data validation error"),
            validatingAttribute: null,
            value: null);
    }

    [HttpGet("customWarning")]
    public IActionResult CustomWarning()
    {
        throw new CustomAllegroWarningException("Custom warning exception");
    }

    [HttpGet("customError")]
    public IActionResult CustomError()
    {
        throw new CustomAllegroErrorException("Custom error exception");
    }

    [HttpGet("customErrorWithData")]
    public IActionResult CustomErrorWithData()
    {
        throw new CustomAllegroErrorWithDataException(
            "Custom error with data exception",
            new CustomAllegroErrorWithDataException.ErrorData("data 1", "data 2"));
    }

    [HttpGet("customErrorWithCustomResponse")]
    public IActionResult CustomErrorWithCustomResponse()
    {
        throw new CustomAllegroWithCustomResponseException("Custom error with custom error");
    }

    [HttpGet("customErrorBasedOnException")]
    public IActionResult CustomErrorBasedOnException()
    {
        throw new ApiException1()
        {
            ResponseCode = 499,
            ExceptionCode = "code1",
            ExceptionMessage = "message1"
        };
    }

    [HttpPost("defaultModelStateValidation/{id}")]
    public IActionResult DefaultModelStateValidation(
        [FromRoute][Required] string id,
        [FromBody][Required] DataToValidate data)
    {
        return Ok("Remove body or send empty Id or Id2 in body to see how model validation is handled");
    }

    [HttpPost("customModelStateValidation/{id}")]
    public IActionResult CustomModelStateValidation(
        [FromRoute][Required] string id,
        [FromBody][Required] DataToValidate data)
    {
        // In ErrorHandlingConfigurationBuilder WithCustomModelStateValidationHandler is used and verify action context, if apply returns custom error defined
        return Ok("Remove body or send empty Id or Id2 in body to see how model validation is handled");
    }

    public record DataToValidate([Required] string Id, [Required] string Id2, string Id3);
}

internal static class CustomAllegroExceptionHandlerBuilder
{
    internal static ErrorHandlingConfigurationBuilder WithCustomAllegroErrorHandling(
        this ErrorHandlingConfigurationBuilder builder)
    {
        return builder
            .WithAdditionalInstrumentation(context =>
            {
                Console.WriteLine("Custom instrumentation, for example SerilogEnricher");
                return new CustomInstrumentation();
            })
            .WithCustomHandler<CustomAllegroWarningException>(
                ex => Error
                    .Create(LogLevel.Warning, (int)HttpStatusCode.BadRequest)
                    .AddErrorData(nameof(CustomAllegroWarningException), ex.Message, ex.Message))
            .WithCustomHandler<CustomAllegroErrorException>(
                ex => Error
                    .Create(LogLevel.Error, (int)HttpStatusCode.InternalServerError)
                    .AddErrorData(nameof(CustomAllegroErrorException), ex.Message, ex.Message))
            .WithCustomHandler<CustomAllegroErrorWithDataException>(
                ex => Error
                    .Create(LogLevel.Error, (int)HttpStatusCode.UnprocessableEntity)
                    .AddErrorData(nameof(CustomAllegroErrorWithDataException), ex.Message, ex.Message)
                    .AddErrorData(
                        nameof(CustomAllegroErrorWithDataException.ErrorData.Data1),
                        ex.AdditionalData.Data1,
                        ex.AdditionalData.Data1))
            .WithCustomHandler<CustomAllegroWithCustomResponseException>(
                ex => Error
                    .Create(LogLevel.NoLog, (int)HttpStatusCode.Conflict)
                    .WithCustomResponse(new { Data = "Custom resposne data" }))
            .WithCustomHandler<ApiException>(ex =>
                Error.Create(LogLevel.Error, ex.ResponseCode)
                    .AddErrorData(ex.ExceptionCode ?? "not set", ex.ExceptionMessage ?? "not set", string.Empty))
            .WithCustomModelStateValidationErrorHandler(context =>
            {
                if (context.ActionDescriptor.DisplayName == "Allegro.Extensions.AspNetCore.Demo.Controllers.ErrorHandlingController.CustomModelStateValidation (Allegro.Extensions.AspNetCore.Demo)")
                {
                    return Error.Create(LogLevel.NoLog, StatusCodes.Status423Locked)
                        .WithCustomResponse(new { ErrorData1 = "asd" });
                }

                return null;
            });
    }
}

internal class CustomAllegroWarningException : Exception
{
    public CustomAllegroWarningException(string message) : base(message)
    {
    }
}

internal class CustomAllegroErrorException : Exception
{
    public CustomAllegroErrorException(string message) : base(message)
    {
    }
}

internal class CustomAllegroErrorWithDataException : Exception
{
    public ErrorData AdditionalData { get; }

    public CustomAllegroErrorWithDataException(string message, ErrorData errorData) : base(message)
    {
        AdditionalData = errorData;
    }

    public record ErrorData(string Data1, string Data2);
}

internal class CustomAllegroWithCustomResponseException : Exception
{
    public CustomAllegroWithCustomResponseException(string message) : base(message)
    {
    }
}

internal class CustomInstrumentation : IDisposable
{
    public void Dispose()
    {
    }
}

internal class ApiException : Exception
{
    public int ResponseCode { get; init; }
    public string? ExceptionCode { get; init; }
    public string? ExceptionMessage { get; init; }
}

internal class ApiException1 : ApiException { }
internal class ApiException2 : ApiException { }
internal class ApiException3 : ApiException { }
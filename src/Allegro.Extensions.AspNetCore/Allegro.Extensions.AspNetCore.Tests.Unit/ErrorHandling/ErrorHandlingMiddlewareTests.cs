using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allegro.Extensions.AspNetCore.ErrorHandling;
using Allegro.Extensions.AspNetCore.ErrorHandling.Internals;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Allegro.Extensions.AspNetCore.Tests.Unit.ErrorHandling;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task By_default_on_error_exception_log_error_should_be_executed()
    {
        using var fixture = new Fixture();
        var middleware = fixture.Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new Exception());

        fixture.VerifyLogErrorExecuted(times: 1);
        fixture.VerifyLogWarningExecuted(times: 0);
    }

    [Fact]
    public async Task By_default_on_error_exception_server_error_status_code_should_be_set()
    {
        using var fixture = new Fixture();
        var middleware = fixture.Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new Exception());

        fixture.VerifyStatusCode(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task By_default_on_validation_exception_bad_request_status_code_should_be_set()
    {
        using var fixture = new Fixture();
        var middleware = fixture.Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new ValidationException());

        fixture.VerifyStatusCode(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task By_default_on_validation_exception_log_warning_should_be_executed()
    {
        using var fixture = new Fixture();
        var middleware = fixture.Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new ValidationException());

        fixture.VerifyLogWarningExecuted(times: 1);
        fixture.VerifyLogErrorExecuted(times: 0);
    }

    [Theory]
    [InlineData(LogLevel.Error, 1, 0)]
    [InlineData(LogLevel.Warning, 0, 1)]
    [InlineData(LogLevel.NoLog, 0, 0)]
    public async Task When_custom_error_handling_occured_configured_log_should_be_executed(
        LogLevel logLevel,
        int errorLogExecutedTimes,
        int warningLogExecutedTimes)
    {
        using var fixture = new Fixture();
        var middleware = fixture
            .WithCustomLogLevelError(logLevel)
            .Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new CustomException());

        fixture.VerifyLogWarningExecuted(warningLogExecutedTimes);
        fixture.VerifyLogErrorExecuted(errorLogExecutedTimes);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    [InlineData(455)]
    [InlineData(0)]
    public async Task When_custom_error_handling_occured_configured_status_code_should_be_returned(int statusCode)
    {
        using var fixture = new Fixture();
        var middleware = fixture
            .WithCustomStatusCode(statusCode)
            .Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new CustomException());

        fixture.VerifyStatusCode(statusCode);
    }

    [Fact]
    public async Task Derived_exception_will_be_executed_with_base_exception_error_handler()
    {
        using var fixture = new Fixture();
        var middleware = fixture
            .WithCustomLogLevelError(LogLevel.Warning)
            .Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new DerivedCustomException());

        fixture.VerifyLogErrorExecuted(times: 0);
        fixture.VerifyLogWarningExecuted(times: 1);
    }

    [Fact]
    public async Task Error_response_body_should_be_standardize()
    {
        using var fixture = new Fixture();
        var middleware = fixture
            .Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new Exception());

        fixture.VerifyStandardResponseMessage();
    }

    [Fact]
    public async Task Custom_error_data_should_be_written_to_response()
    {
        var error = Error
            .Create(LogLevel.Error, StatusCodes.Status400BadRequest)
            .AddErrorData("Code", "Message", "UserMessage", "Path", "Details")
            .AddErrorData("Code1", "Message1", "UserMessage1", "Path1", "Details1");

        using var fixture = new Fixture();
        var middleware = fixture
            .WithCustomError(error)
            .Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new CustomException());

        fixture.VerifyAllErrorDataWasWrittenToResponse(error);
    }

    [Fact]
    public async Task Custom_error_with_custom_response_body_should_be_written_to_response()
    {
        var customResponse = new { Data1 = "data1", Data2 = 1 };
        var error = Error
            .Create(LogLevel.Error, StatusCodes.Status400BadRequest)
            .WithCustomResponse(customResponse);

        using var fixture = new Fixture();
        var middleware = fixture
            .WithCustomError(error)
            .Configure();

        await middleware.InvokeAsync(fixture.Context, context => throw new CustomException());

        fixture.VerifyCustomDataWrittenToResponse(customResponse);
    }

    private class CustomException : Exception
    {
    }

    private class DerivedCustomException : CustomException
    {
    }

    private class Fixture : IDisposable
    {
        public void Dispose()
        {
            _responseBodyStream.Dispose();
        }

        private readonly List<(string Message, Exception Exception)> _exceptionLogs = new();
        private readonly List<(string Message, Exception Exception)> _warningLogs = new();

        private readonly Dictionary<Type, Func<Exception, Error>> _customErrorHandlerMap = new();

        public HttpContext Context { get; } = new DefaultHttpContext();
        private readonly MemoryStream _responseBodyStream = new();

        public ErrorHandlingMiddleware Configure()
        {
            Context.Response.Body = _responseBodyStream;
            return new ErrorHandlingMiddleware(
                _customErrorHandlerMap,
                ImmutableArray<Func<HttpContext, IDisposable>>.Empty,
                new SystemTextJsonWebErrorSerializer(),
                logError: error => _exceptionLogs.Add(error),
                logWarning: warning => _warningLogs.Add(warning)
            );
        }

        public Fixture WithCustomLogLevelError(LogLevel logLevel)
        {
            _customErrorHandlerMap.Add(typeof(CustomException), exception => Error.Create(logLevel, 0));
            return this;
        }

        public Fixture WithCustomStatusCode(int statusCode)
        {
            _customErrorHandlerMap.Add(
                typeof(CustomException),
                exception => Error.Create(LogLevel.Warning, statusCode));
            return this;
        }

        public Fixture WithCustomError(Error error)
        {
            _customErrorHandlerMap.Add(
                typeof(CustomException),
                exception => error);
            return this;
        }

        public void VerifyLogErrorExecuted(int times) => _exceptionLogs.Count.Should().Be(times);
        public void VerifyLogWarningExecuted(int times) => _warningLogs.Count.Should().Be(times);

        public void VerifyStatusCode(int statusCode)
        {
            Context.Response.StatusCode.Should().Be(statusCode);
        }

        public void VerifyAllErrorDataWasWrittenToResponse(Error error)
        {
            var errorResponse = GetErrorResponse();

            errorResponse.Errors.Should().BeEquivalentTo(error.Errors.Select(e =>
                new ErrorResponse(e.Code, e.Message, e.UserMessage, e.Path, e.Details)));
        }

        public void VerifyStandardResponseMessage()
        {
            const string standardErrorResponseBody =
                "{\"errors\":[{\"code\":\"UnhandledException\",\"message\":\"Unhandled Exception\",\"userMessage\":null,\"path\":null,\"details\":null}]}";
            var bodyText = GetResponseBodyAsText();

            bodyText.Should().Be(standardErrorResponseBody);
        }

        public void VerifyCustomDataWrittenToResponse(object customResponse)
        {
            var responseBody = GetResponseBodyAsText();
            var customResponseSerialized = JsonSerializer.Serialize(customResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            responseBody.Should().Be(customResponseSerialized);
        }

        private string GetResponseBodyAsText()
        {
            _responseBodyStream.Position = 0;
            var reader = new StreamReader(_responseBodyStream);
            var bodyText = reader.ReadToEnd();
            return bodyText;
        }

        private ErrorResponsesHolder GetErrorResponse()
        {
            _responseBodyStream.Position = 0;
            return JsonSerializer.Deserialize<ErrorResponsesHolder>(
                _responseBodyStream,
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        }
    }
}
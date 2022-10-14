using Allegro.Extensions.AspNetCore.ErrorHandling;
using Allegro.Extensions.AspNetCore.ErrorHandling.Internals;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Allegro.Extensions.AspNetCore.Tests.Unit.ErrorHandling;

public class InvalidModelStateResponseFactoryTests
{
    private const string TheErrorMessage = "Empty id";

    [Fact]
    public void By_default_model_state_errors_returns_bad_request_with_standardize_response_body()
    {
        var factory = new InvalidModelStateResponseFactory();
        var actionContext = CreateInvalidModelStateActionContext();

        var actionResult = (ObjectResult)factory.BuildResponse(actionContext);

        actionResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        actionResult.Value.Should().BeEquivalentTo(TheErrorResponsesHolder);
    }

    [Fact]
    public void Able_to_handle_error_with_custom_behaviour()
    {
        var customResponse = new { Data1 = "Data1", Data2 = 2 };
        var error = Error
            .Create(LogLevel.NoLog, StatusCodes.Status410Gone)
            .WithCustomResponse(customResponse);
        var factory = new InvalidModelStateResponseFactory(context => error);
        var actionContext = CreateInvalidModelStateActionContext();

        var actionResult = (ObjectResult)factory.BuildResponse(actionContext);

        actionResult.StatusCode.Should().Be(StatusCodes.Status410Gone);
        actionResult.Value.Should().Be(customResponse);
    }

    private static ActionContext CreateInvalidModelStateActionContext()
    {
        var modelStateDictionary = new ModelStateDictionary();
        modelStateDictionary.AddModelError("Id", TheErrorMessage);
        return new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            modelStateDictionary);
    }

    private static readonly ErrorResponsesHolder TheErrorResponsesHolder =
        new ErrorResponsesHolder(new[] { new ErrorResponse(ModelValidationState.Invalid.ToString(), TheErrorMessage) });
}
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Allegro.Extensions.AspNetCore.ErrorHandling.Internals
{
    internal class InvalidModelStateResponseFactory
    {
        private readonly Func<ActionContext, Error?>? _customModelStateValidationHandler;

        public InvalidModelStateResponseFactory(Func<ActionContext, Error?>? customModelStateValidationHandler = null)
        {
            _customModelStateValidationHandler = customModelStateValidationHandler;
        }

        public IActionResult BuildResponse(ActionContext context)
        {
            var errorResponse = _customModelStateValidationHandler is not null
                ? HandleCustomResponse(context)
                : DefaultResponse(context);

            return new ObjectResult(errorResponse.Response) { StatusCode = errorResponse.StatusCode };
        }

        private static (int StatusCode, object Response) DefaultResponse(ActionContext context)
        {
            var errorResponseHolder = new ErrorResponsesHolder(
                Errors: context.ModelState.Values.Select(
                    modelStateEntry => new ErrorResponse(
                        modelStateEntry.ValidationState.ToString(),
                        GetErrorMessageFrom(modelStateEntry)
                    )));
            return (StatusCodes.Status400BadRequest, errorResponseHolder);
        }

        private (int StatusCode, object Response) HandleCustomResponse(ActionContext context)
        {
            var error = _customModelStateValidationHandler!(context);
            return error is not null
                ? (error.ResponseCode, error.BuildErrorResponse())
                : DefaultResponse(context);
        }

        private static string GetErrorMessageFrom(ModelStateEntry value)
        {
            return string.Join(", ", value.Errors.Select(e => e.ErrorMessage));
        }
    }
}
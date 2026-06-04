using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;
using System.Diagnostics;

namespace MyOS.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext httpContext,
            IErrorTranslator errorTranslator, ICurrentUser currentUser)
        {
            if (result.IsSuccess)
            {
                if (result.Value is Unit)
                    return new NoContentResult();

                return new OkObjectResult(result.Value);
            }

            var message = errorTranslator.Translate(result.Error, currentUser.Language);
            var problem = CreateProblemDetails(result.Error, message, httpContext);

            return new ObjectResult(problem)
            {
                StatusCode = problem.Status
            };
        }

        private static ProblemDetails CreateProblemDetails(Error error, string message, HttpContext httpContext)
        {
            var status = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = error.Type.ToString(),
                Detail = message,
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] =
                Activity.Current?.Id ?? httpContext.TraceIdentifier;

            problem.Extensions["correlationId"] =
                httpContext.TraceIdentifier;

            problem.Extensions["errorCode"] =
                error.Code;

            return problem;
        }
    }
}

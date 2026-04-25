using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyOS.Core.Application.Abstractions.Results;
using System.Diagnostics;

namespace MyOS.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext httpContext)
        {
            if (result.IsSuccess)
            {
                if (result.Value is Unit)
                {
                    return new NoContentResult();
                }

                return new OkObjectResult(result.Value);
            }

            var problem = CreateProblemDetails(result.Error, httpContext);

            return new ObjectResult(problem)
            {
                StatusCode = problem.Status
            };
        }

        private static ProblemDetails CreateProblemDetails(Error error, HttpContext httpContext)
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
                Detail = error.Message,
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

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Executing {RequestName} {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        if (response.IsFailure)
            logger.LogWarning("Failed {RequestName} in {ElapsedMs}ms — {ErrorCode}",
                requestName, stopwatch.ElapsedMilliseconds, response.Error.Code);
        else
            logger.LogInformation("Completed {RequestName} in {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}

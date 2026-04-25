using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, Result<TResponse>> where TRequest : notnull
    {
        public async Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<Result<TResponse>> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count == 0)
            {
                return await next(cancellationToken);
            }

            var message = string.Join(
                Environment.NewLine,
                failures.Select(f => f.ErrorMessage));

            var error = Error.Validation(
                "Validation.Failed",
                message);

            return Result<TResponse>.Failure(error);
        }
    }
}

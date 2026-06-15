using MediatR;
using MyOS.Core.Application.Abstractions.Results;

namespace MyOS.Core.Application.Abstractions.BusinessRules
{
    public static class BusinessRuleChecker
    {
        public static async Task<Result<Unit>> CheckAsync(
            CancellationToken cancellationToken, params IBusinessRule[] rules)
        {
            foreach (var rule in rules)
            {
                if (!await rule.CheckAsync(cancellationToken))
                    return Result<Unit>.Failure(rule.Error);
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}

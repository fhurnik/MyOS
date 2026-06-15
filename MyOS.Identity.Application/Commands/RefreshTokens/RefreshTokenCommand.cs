using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Commands.Shared.BusinesRules;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.RefreshTokens
{
    public sealed record RefreshTokenCommand(string Token) : ICommand<AuthTokens>;

    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }

    internal sealed class RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IAuthTokenIssuer authTokenIssuer,
        IUnitOfWork unitOfWork) : ICommandHandler<RefreshTokenCommand, AuthTokens>
    {
        public async Task<Result<AuthTokens>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var existingToken = await userRepository.GetRefreshTokenAsync(command.Token, cancellationToken);
            var user = existingToken is not null
                ? await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
                : null;

            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new RefreshTokenMustBeActiveRule(existingToken),
                new UserMustBeActiveRule(user));

            if (check.IsFailure)
                return Result<AuthTokens>.Failure(check.Error);

            var tokens = await authTokenIssuer.IssueAsync(user!, cancellationToken);
            existingToken!.Revoke(replacedByToken: tokens.RefreshToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(tokens);
        }
    }
}

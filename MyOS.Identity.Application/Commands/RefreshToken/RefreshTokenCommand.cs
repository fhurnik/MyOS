using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.RefreshToken
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
            if (existingToken is null || !existingToken.IsActive)
                return Result<AuthTokens>.Failure(UserErrors.InvalidRefreshToken);

            var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
            if (user is null || !user.IsActive)
                return Result<AuthTokens>.Failure(UserErrors.AccountDisabled);

            var tokens = await authTokenIssuer.IssueAsync(user, cancellationToken);
            existingToken.Revoke(replacedByToken: tokens.RefreshToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(tokens);
        }
    }
}

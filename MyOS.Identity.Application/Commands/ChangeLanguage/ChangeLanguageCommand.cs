using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Domain.Enums;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.ChangeLanguage
{
    public sealed record ChangeLanguageCommand(Language Language, string RefreshToken) : ICommand<AuthTokens>;

    public sealed class ChangeLanguageCommandValidator : AbstractValidator<ChangeLanguageCommand>
    {
        public ChangeLanguageCommandValidator()
        {
            RuleFor(x => x.Language).IsInEnum();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    internal sealed class ChangeLanguageCommandHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        IAuthTokenIssuer authTokenIssuer,
        IUnitOfWork unitOfWork) : ICommandHandler<ChangeLanguageCommand, AuthTokens>
    {
        public async Task<Result<AuthTokens>> Handle(ChangeLanguageCommand command, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(currentUser.Id, cancellationToken);
            if (user is null || !user.IsActive)
                return Result<AuthTokens>.Failure(UserErrors.AccountDisabled);

            var existingToken = await userRepository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);
            if (existingToken is null || !existingToken.IsActive || existingToken.UserId != user.Id)
                return Result<AuthTokens>.Failure(UserErrors.InvalidRefreshToken);

            user.ChangeLanguage(command.Language);

            var tokens = await authTokenIssuer.IssueAsync(user, cancellationToken);
            existingToken.Revoke(replacedByToken: tokens.RefreshToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(tokens);
        }
    }
}

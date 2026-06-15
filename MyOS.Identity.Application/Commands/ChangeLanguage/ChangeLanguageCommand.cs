using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Domain.Enums;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Commands.Shared.BusinesRules;
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
            var existingToken = await userRepository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);

            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new UserMustBeActiveRule(user),
                new RefreshTokenMustBeActiveRule(existingToken, user?.Id));

            if (check.IsFailure)
                return Result<AuthTokens>.Failure(check.Error);

            user!.ChangeLanguage(command.Language);

            var tokens = await authTokenIssuer.IssueAsync(user, cancellationToken);
            existingToken!.Revoke(replacedByToken: tokens.RefreshToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(tokens);
        }
    }
}

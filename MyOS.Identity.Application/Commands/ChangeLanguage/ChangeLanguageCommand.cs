using FluentValidation;
using Microsoft.Extensions.Options;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Domain.Enums;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;
using DomainRefreshToken = MyOS.Identity.Domain.Users.RefreshToken;

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
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings) : ICommandHandler<ChangeLanguageCommand, AuthTokens>
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

            var accessToken = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, command.Language);
            var rawRefreshToken = jwtTokenGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays);

            existingToken.Revoke(replacedByToken: rawRefreshToken);

            var refreshToken = DomainRefreshToken.Create(user.Id, rawRefreshToken, expiresAt);
            await userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(new AuthTokens(accessToken, rawRefreshToken));
        }
    }
}

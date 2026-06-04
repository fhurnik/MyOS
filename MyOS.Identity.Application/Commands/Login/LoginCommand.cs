using FluentValidation;
using Microsoft.Extensions.Options;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;
using DomainRefreshToken = MyOS.Identity.Domain.Users.RefreshToken;

namespace MyOS.Identity.Application.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : ICommand<AuthTokens>;

    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    internal sealed class LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings) : ICommandHandler<LoginCommand, AuthTokens>
    {
        public async Task<Result<AuthTokens>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
                return Result<AuthTokens>.Failure(UserErrors.InvalidCredentials);

            if (!user.IsActive)
                return Result<AuthTokens>.Failure(UserErrors.AccountDisabled);

            var accessToken = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email);
            var rawRefreshToken = jwtTokenGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays);

            var refreshToken = DomainRefreshToken.Create(user.Id, rawRefreshToken, expiresAt);
            await userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(new AuthTokens(accessToken, rawRefreshToken));
        }
    }
}

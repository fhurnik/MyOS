using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

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
        IAuthTokenIssuer authTokenIssuer,
        IUnitOfWork unitOfWork) : ICommandHandler<LoginCommand, AuthTokens>
    {
        public async Task<Result<AuthTokens>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
                return Result<AuthTokens>.Failure(UserErrors.InvalidCredentials);

            if (!user.IsActive)
                return Result<AuthTokens>.Failure(UserErrors.AccountDisabled);

            var tokens = await authTokenIssuer.IssueAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthTokens>.Success(tokens);
        }
    }
}

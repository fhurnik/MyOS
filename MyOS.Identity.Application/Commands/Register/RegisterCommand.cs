using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.Exceptions;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Register.BusinesRules;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.Register
{
    public sealed record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password) : ICommand<Guid>;

    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        }
    }

    internal sealed class RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork) : ICommandHandler<RegisterCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new EmailMustBeUniqueRule(userRepository, command.Email));

            if (check.IsFailure)
                return Result<Guid>.Failure(check.Error);

            var passwordHash = passwordHasher.Hash(command.Password);
            var user = User.Create(command.FirstName, command.LastName, command.Email, passwordHash);

            await userRepository.AddAsync(user, cancellationToken);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (UniqueConstraintViolationException)
            {
                return Result<Guid>.Failure(UserErrors.EmailAlreadyInUse with
                {
                    Parameters = new Dictionary<string, string> { ["email"] = command.Email }
                });
            }

            return Result<Guid>.Success(user.Id);
        }
    }
}

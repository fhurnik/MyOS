using MyOS.Core.Application.Abstractions;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Login;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.UnitTests.Application.Identity.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
        private readonly IAuthTokenIssuer _issuer = Substitute.For<IAuthTokenIssuer>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

        private LoginCommandHandler CreateHandler() =>
            new(_users, _hasher, _issuer, _unitOfWork);

        private static User ActiveUser() =>
            User.Create("Jan", "Kowalski", "jan@myos.local", "stored-hash");

        [Fact]
        public async Task Handle_UnknownEmail_ReturnsInvalidCredentialsAndDoesNotSave()
        {
            _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

            var result = await CreateHandler().Handle(new LoginCommand("x@myos.local", "pw"), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.InvalidCredentials);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WrongPassword_ReturnsInvalidCredentialsAndDoesNotSave()
        {
            var user = ActiveUser();
            _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
            _hasher.Verify("wrong", user.PasswordHash).Returns(false);

            var result = await CreateHandler().Handle(new LoginCommand(user.Email, "wrong"), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.InvalidCredentials);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DisabledAccountWithCorrectPassword_ReturnsAccountDisabledAndDoesNotSave()
        {
            var user = ActiveUser();
            user.ChangeActiveStatus(false);
            _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
            _hasher.Verify(Arg.Any<string>(), user.PasswordHash).Returns(true);

            var result = await CreateHandler().Handle(new LoginCommand(user.Email, "pw"), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.AccountDisabled);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidCredentials_IssuesTokensAndSavesOnce()
        {
            var user = ActiveUser();
            _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
            _hasher.Verify(Arg.Any<string>(), user.PasswordHash).Returns(true);
            _issuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(new AuthTokens("access", "refresh"));

            var result = await CreateHandler().Handle(new LoginCommand(user.Email, "pw"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.AccessToken.ShouldBe("access");
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}

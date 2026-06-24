using MyOS.Core.Application.Abstractions;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.RefreshTokens;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.UnitTests.Application.Identity.RefreshTokens
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly IAuthTokenIssuer _issuer = Substitute.For<IAuthTokenIssuer>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

        private RefreshTokenCommandHandler CreateHandler() =>
            new(_users, _issuer, _unitOfWork);

        private static RefreshToken ActiveTokenFor(User user) =>
            RefreshToken.Create(user.Id, "stored-refresh", DateTime.UtcNow.AddDays(7));

        [Fact]
        public async Task Handle_UnknownToken_ReturnsInvalidRefreshTokenAndDoesNotSave()
        {
            _users.GetRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

            var result = await CreateHandler().Handle(new RefreshTokenCommand("ghost"), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.InvalidRefreshToken);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ActiveTokenButDisabledUser_ReturnsAccountDisabledAndDoesNotSave()
        {
            var user = User.Create("Jan", "Kowalski", "jan@myos.local", "h");
            user.ChangeActiveStatus(false);
            var token = ActiveTokenFor(user);
            _users.GetRefreshTokenAsync(token.Token, Arg.Any<CancellationToken>()).Returns(token);
            _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

            var result = await CreateHandler().Handle(new RefreshTokenCommand(token.Token), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.AccountDisabled);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ActiveTokenAndUser_RotatesTokenAndSavesOnce()
        {
            var user = User.Create("Jan", "Kowalski", "jan@myos.local", "h");
            var token = ActiveTokenFor(user);
            _users.GetRefreshTokenAsync(token.Token, Arg.Any<CancellationToken>()).Returns(token);
            _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
            _issuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(new AuthTokens("access", "new-refresh"));

            var result = await CreateHandler().Handle(new RefreshTokenCommand(token.Token), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            token.IsRevoked.ShouldBeTrue();                 // old token rotated out
            token.ReplacedByToken.ShouldBe("new-refresh");
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}

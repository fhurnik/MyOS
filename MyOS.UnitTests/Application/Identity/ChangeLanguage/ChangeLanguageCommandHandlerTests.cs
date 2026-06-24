using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.ChangeLanguage;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Identity.ChangeLanguage
{
    public class ChangeLanguageCommandHandlerTests
    {
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly IAuthTokenIssuer _issuer = Substitute.For<IAuthTokenIssuer>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private ChangeLanguageCommandHandler CreateHandler() =>
            new(_currentUser, _users, _issuer, _unitOfWork);

        private static User EnglishUser() =>
            User.Create("Jan", "Kowalski", "jan@myos.local", "h", Language.English);

        [Fact]
        public async Task Handle_UserNotFound_ReturnsAccountDisabledAndDoesNotSave()
        {
            _users.GetByIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns((User?)null);
            _users.GetRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

            var result = await CreateHandler().Handle(
                new ChangeLanguageCommand(Language.Polish, "tok"), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.AccountDisabled);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ActiveUserButUnknownToken_ReturnsInvalidRefreshTokenAndDoesNotSave()
        {
            var user = EnglishUser();
            _users.GetByIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns(user);
            _users.GetRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

            var result = await CreateHandler().Handle(
                new ChangeLanguageCommand(Language.Polish, "tok"), CancellationToken.None);

            result.Error.ShouldBe(UserErrors.InvalidRefreshToken);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ActiveUserAndToken_ChangesLanguageRotatesTokenAndSavesOnce()
        {
            var user = EnglishUser();
            var token = RefreshToken.Create(user.Id, "tok", DateTime.UtcNow.AddDays(7));
            _users.GetByIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns(user);
            _users.GetRefreshTokenAsync("tok", Arg.Any<CancellationToken>()).Returns(token);
            _issuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(new AuthTokens("access", "new-refresh"));

            var result = await CreateHandler().Handle(
                new ChangeLanguageCommand(Language.Polish, "tok"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            user.Language.ShouldBe(Language.Polish);
            token.IsRevoked.ShouldBeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}

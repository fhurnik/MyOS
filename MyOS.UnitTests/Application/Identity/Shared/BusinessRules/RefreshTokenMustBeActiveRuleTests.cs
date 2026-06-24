using MyOS.Identity.Application.Commands.Shared.BusinesRules;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.UnitTests.Application.Identity.Shared.BusinessRules
{
    public class RefreshTokenMustBeActiveRuleTests
    {
        private static RefreshToken ActiveToken(Guid userId) =>
            RefreshToken.Create(userId, "tok", DateTime.UtcNow.AddDays(7));

        [Fact]
        public async Task CheckAsync_ActiveTokenNoOwnerCheck_Passes()
        {
            var rule = new RefreshTokenMustBeActiveRule(ActiveToken(Guid.NewGuid()));

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_ActiveTokenMatchingOwner_Passes()
        {
            var userId = Guid.NewGuid();
            var rule = new RefreshTokenMustBeActiveRule(ActiveToken(userId), expectedUserId: userId);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_NullToken_Fails()
        {
            var rule = new RefreshTokenMustBeActiveRule(null);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
            rule.Error.ShouldBe(UserErrors.InvalidRefreshToken);
        }

        [Fact]
        public async Task CheckAsync_RevokedToken_Fails()
        {
            var token = ActiveToken(Guid.NewGuid());
            token.Revoke();
            var rule = new RefreshTokenMustBeActiveRule(token);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
        }

        [Fact]
        public async Task CheckAsync_ActiveTokenButWrongOwner_Fails()
        {
            var rule = new RefreshTokenMustBeActiveRule(ActiveToken(Guid.NewGuid()), expectedUserId: Guid.NewGuid());

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
        }
    }
}

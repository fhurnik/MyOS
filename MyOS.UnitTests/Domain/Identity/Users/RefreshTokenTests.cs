using MyOS.Identity.Domain.Users;

namespace MyOS.UnitTests.Domain.Identity.Users
{
    // IsActive is the composite decision (!revoked && !expired) that the auth flow relies on.
    public class RefreshTokenTests
    {
        [Fact]
        public void IsActive_FreshUnrevokedToken_IsTrue()
        {
            var token = RefreshToken.Create(Guid.NewGuid(), "tok", DateTime.UtcNow.AddDays(7));

            token.IsActive.ShouldBeTrue();
        }

        [Fact]
        public void IsActive_ExpiredToken_IsFalse()
        {
            var token = RefreshToken.Create(Guid.NewGuid(), "tok", DateTime.UtcNow.AddMinutes(-1));

            token.IsExpired.ShouldBeTrue();
            token.IsActive.ShouldBeFalse();
        }

        [Fact]
        public void IsActive_RevokedToken_IsFalse()
        {
            var token = RefreshToken.Create(Guid.NewGuid(), "tok", DateTime.UtcNow.AddDays(7));

            token.Revoke();

            token.IsRevoked.ShouldBeTrue();
            token.IsActive.ShouldBeFalse();
        }

        [Fact]
        public void Revoke_WithReplacement_RecordsTheSuccessorToken()
        {
            var token = RefreshToken.Create(Guid.NewGuid(), "tok", DateTime.UtcNow.AddDays(7));

            token.Revoke(replacedByToken: "next");

            token.RevokedAtUtc.ShouldNotBeNull();
            token.ReplacedByToken.ShouldBe("next");
        }
    }
}

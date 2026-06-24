using MyOS.Identity.Application.Commands.Shared.BusinesRules;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.UnitTests.Application.Identity.Shared.BusinessRules
{
    public class UserMustBeActiveRuleTests
    {
        private static User ActiveUser() =>
            User.Create("Jan", "Kowalski", "jan@myos.local", "hash");

        [Fact]
        public async Task CheckAsync_ActiveUser_Passes()
        {
            var rule = new UserMustBeActiveRule(ActiveUser());

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_NullUser_Fails()
        {
            var rule = new UserMustBeActiveRule(null);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
            rule.Error.ShouldBe(UserErrors.AccountDisabled);
        }

        [Fact]
        public async Task CheckAsync_DisabledUser_Fails()
        {
            var user = ActiveUser();
            user.ChangeActiveStatus(false);
            var rule = new UserMustBeActiveRule(user);

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
        }
    }
}

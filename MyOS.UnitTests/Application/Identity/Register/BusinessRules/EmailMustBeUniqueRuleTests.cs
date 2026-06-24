using MyOS.Identity.Application.Commands.Register.BusinesRules;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.UnitTests.Application.Identity.Register.BusinessRules
{
    public class EmailMustBeUniqueRuleTests
    {
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();

        [Fact]
        public async Task CheckAsync_NoUserWithEmail_Passes()
        {
            _users.GetByEmailAsync("free@myos.local", Arg.Any<CancellationToken>()).Returns((User?)null);
            var rule = new EmailMustBeUniqueRule(_users, "free@myos.local");

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_EmailAlreadyRegistered_Fails()
        {
            var existing = User.Create("Jan", "Kowalski", "taken@myos.local", "hash");
            _users.GetByEmailAsync("taken@myos.local", Arg.Any<CancellationToken>()).Returns(existing);
            var rule = new EmailMustBeUniqueRule(_users, "taken@myos.local");

            (await rule.CheckAsync(CancellationToken.None)).ShouldBeFalse();
            rule.Error.Code.ShouldBe(UserErrors.EmailAlreadyInUse.Code);
        }
    }
}

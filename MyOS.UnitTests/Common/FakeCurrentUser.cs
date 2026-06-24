using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;

namespace MyOS.UnitTests.Common
{
    // Minimal ICurrentUser test double. Unlike CurrentUserService, Id/Email never throw —
    // tests set them explicitly. Defaults to an authenticated English user.
    internal sealed class FakeCurrentUser : ICurrentUser
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Email { get; init; } = "test@myos.local";
        public bool IsAuthenticated { get; init; } = true;
        public Language Language { get; init; } = Language.English;
    }
}

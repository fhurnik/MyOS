using MyOS.Core.Domain.Entities;

namespace MyOS.Identity.Domain.Users
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public string? ReplacedByToken { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
        public bool IsRevoked => RevokedAtUtc is not null;
        public bool IsActive => !IsRevoked && !IsExpired;

        public static RefreshToken Create(Guid userId, string token, DateTime expiresAtUtc)
            => new(userId, token, expiresAtUtc);

        internal RefreshToken(Guid userId, string token, DateTime expiresAtUtc)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Token = token;
            ExpiresAtUtc = expiresAtUtc;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void Revoke(string? replacedByToken = null)
        {
            RevokedAtUtc = DateTime.UtcNow;
            ReplacedByToken = replacedByToken;
        }

        private RefreshToken()
        {
            // for EF Core
            Token = null!;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Infrastructure.EntityConfigurations.Users
{
    internal sealed class RefreshTokenEntityConfiguration : EntityConfiguration<RefreshToken>
    {
        public override void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens", "identity");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Token).HasColumnName("token").HasMaxLength(500);
            builder.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.RevokedAtUtc).HasColumnName("revoked_at_utc");
            builder.Property(x => x.ReplacedByToken).HasColumnName("replaced_by_token").HasMaxLength(500);

            builder.Ignore(x => x.IsExpired);
            builder.Ignore(x => x.IsRevoked);
            builder.Ignore(x => x.IsActive);
        }
    }
}

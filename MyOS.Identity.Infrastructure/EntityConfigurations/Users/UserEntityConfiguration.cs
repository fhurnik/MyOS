using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Infrastructure.EntityConfigurations.Users
{
    internal class UserEntityConfiguration : EntityConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", "identity");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500);
            builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
            builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
            builder.Property(x => x.IsActive).HasColumnName("is_active");
            builder.Property(x => x.Language).HasColumnName("language");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        }
    }
}

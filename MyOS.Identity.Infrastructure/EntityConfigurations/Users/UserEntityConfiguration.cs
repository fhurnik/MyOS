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
            builder.ToTable("Users", "identity");
            builder.HasKey(x => x.Id);
        }
    }
}

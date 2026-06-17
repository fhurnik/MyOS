using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Infrastructure.EntityConfigurations.Quotas
{
    internal sealed class StorageQuotaEntityConfiguration : EntityConfiguration<StorageQuota>
    {
        public override void Configure(EntityTypeBuilder<StorageQuota> builder)
        {
            builder.ToTable("storage_quotas", "storage");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.MaxBytes).HasColumnName("max_bytes");
            builder.Property(x => x.UsedBytes).HasColumnName("used_bytes");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

            builder.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("IX_storage_quotas_user_id");
        }
    }
}

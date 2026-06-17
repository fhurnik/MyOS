using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Storage.Domain.Folders;

namespace MyOS.Modules.Storage.Infrastructure.EntityConfigurations.Folders
{
    internal sealed class FolderEntityConfiguration : EntityConfiguration<Folder>
    {
        public override void Configure(EntityTypeBuilder<Folder> builder)
        {
            builder.ToTable("folders", "storage");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.ParentId).HasColumnName("parent_id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_folders_user_id");
            builder.HasIndex(x => x.ParentId).HasDatabaseName("IX_folders_parent_id");
        }
    }
}

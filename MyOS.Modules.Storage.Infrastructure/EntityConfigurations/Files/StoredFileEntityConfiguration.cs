using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Storage.Domain.Files;

namespace MyOS.Modules.Storage.Infrastructure.EntityConfigurations.Files
{
    internal sealed class StoredFileEntityConfiguration : EntityConfiguration<StoredFile>
    {
        public override void Configure(EntityTypeBuilder<StoredFile> builder)
        {
            builder.ToTable("files", "storage");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.StorageFileName).HasColumnName("storage_file_name").HasMaxLength(64);
            builder.Property(x => x.OriginalName).HasColumnName("original_name").HasMaxLength(255);
            builder.Property(x => x.Extension).HasColumnName("extension").HasMaxLength(20);
            builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(150);
            builder.Property(x => x.SizeBytes).HasColumnName("size_bytes");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_files_user_id");
        }
    }
}

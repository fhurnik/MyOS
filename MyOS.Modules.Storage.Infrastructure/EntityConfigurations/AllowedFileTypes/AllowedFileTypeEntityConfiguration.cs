using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Storage.Domain.AllowedFileTypes;

namespace MyOS.Modules.Storage.Infrastructure.EntityConfigurations.AllowedFileTypes
{
    internal sealed class AllowedFileTypeEntityConfiguration : EntityConfiguration<AllowedFileType>
    {
        public override void Configure(EntityTypeBuilder<AllowedFileType> builder)
        {
            builder.ToTable("allowed_file_types", "storage");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Extension).HasColumnName("extension").HasMaxLength(20);
            builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(150);
            builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(20);
            builder.Property(x => x.IsActive).HasColumnName("is_active");

            builder.HasIndex(x => x.Extension).IsUnique().HasDatabaseName("IX_allowed_file_types_extension");
        }
    }
}

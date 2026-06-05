using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Infrastructure.EntityConfigurations.CheckList
{
    internal sealed class CheckListEntityConfiguration : EntityConfiguration<Domain.Notes.CheckList.CheckList>
    {
        public override void Configure(EntityTypeBuilder<Domain.Notes.CheckList.CheckList> builder)
        {
            builder.ToTable("check_lists", "notes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(500);
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_check_lists_user_id");

            builder.HasMany<CheckListItem>(x => x.Items)
                .WithOne()
                .HasForeignKey(i => i.CheckListId);

            builder.Navigation(x => x.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

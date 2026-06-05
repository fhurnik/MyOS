using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Infrastructure.EntityConfigurations.CheckList
{
    internal sealed class CheckListItemEntityConfiguration : EntityConfiguration<CheckListItem>
    {
        public override void Configure(EntityTypeBuilder<CheckListItem> builder)
        {
            builder.ToTable("check_list_items", "notes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.CheckListId).HasColumnName("check_list_id");
            builder.Property(x => x.Text).HasColumnName("text").HasMaxLength(2000);
            builder.Property(x => x.IsChecked).HasColumnName("is_checked");
            builder.Property(x => x.Order).HasColumnName("order");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasIndex(x => new { x.CheckListId, x.Order })
                .HasDatabaseName("IX_check_list_items_check_list_id_order");
        }
    }
}

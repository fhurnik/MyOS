using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;

namespace MyOS.Modules.Notes.Infrastructure.EntityConfigurations.TextNotes
{
    internal sealed class TextNoteEntityConfiguration : EntityConfiguration<TextNote>
    {
        public override void Configure(EntityTypeBuilder<TextNote> builder)
        {
            builder.ToTable("text_notes", "notes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(500);
            builder.Property(x => x.Text).HasColumnName("text");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_text_notes_user_id");
        }
    }
}

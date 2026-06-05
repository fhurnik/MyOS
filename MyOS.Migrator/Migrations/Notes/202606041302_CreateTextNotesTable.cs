using FluentMigrator;

namespace MyOS.Migrator.Migrations.Notes
{
    [Migration(202606041302)]
    public sealed class CreateTextNotesTable : Migration
    {
        public override void Up()
        {
            Create.Table("text_notes")
                .InSchema("notes")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("title")
                    .AsString(500)
                    .NotNullable()

                .WithColumn("text")
                    .AsString(int.MaxValue)
                    .NotNullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("updated_at_utc")
                    .AsDateTime2()
                    .Nullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_text_notes_user_id")
                .OnTable("text_notes").InSchema("notes")
                .OnColumn("user_id").Ascending();
        }

        public override void Down()
        {
            Delete.Table("text_notes").InSchema("notes");
        }
    }
}

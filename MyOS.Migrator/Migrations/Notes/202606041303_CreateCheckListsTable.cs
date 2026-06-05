using FluentMigrator;

namespace MyOS.Migrator.Migrations.Notes
{
    [Migration(202606041303)]
    public sealed class CreateCheckListsTable : Migration
    {
        public override void Up()
        {
            Create.Table("check_lists")
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

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("updated_at_utc")
                    .AsDateTime2()
                    .Nullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_check_lists_user_id")
                .OnTable("check_lists").InSchema("notes")
                .OnColumn("user_id").Ascending();
        }

        public override void Down()
        {
            Delete.Table("check_lists").InSchema("notes");
        }
    }
}

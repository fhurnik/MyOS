using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    [Migration(202606171205)]
    public sealed class CreateFoldersTable : Migration
    {
        public override void Up()
        {
            Create.Table("folders")
                .InSchema("storage")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("parent_id")
                    .AsGuid()
                    .Nullable()

                .WithColumn("name")
                    .AsString(255)
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

            Create.Index("ix_folders_user_id")
                .OnTable("folders").InSchema("storage")
                .OnColumn("user_id").Ascending();

            Create.Index("ix_folders_parent_id")
                .OnTable("folders").InSchema("storage")
                .OnColumn("parent_id").Ascending();

            Create.ForeignKey("fk_folders_user_id")
                .FromTable("folders").InSchema("storage").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");

            Create.ForeignKey("fk_folders_parent_id")
                .FromTable("folders").InSchema("storage").ForeignColumn("parent_id")
                .ToTable("folders").InSchema("storage").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_folders_parent_id").OnTable("folders").InSchema("storage");
            Delete.ForeignKey("fk_folders_user_id").OnTable("folders").InSchema("storage");
            Delete.Table("folders").InSchema("storage");
        }
    }
}

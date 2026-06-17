using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    [Migration(202606171204)]
    public sealed class CreateFilesTable : Migration
    {
        public override void Up()
        {
            Create.Table("files")
                .InSchema("storage")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("storage_file_name")
                    .AsString(64)
                    .NotNullable()

                .WithColumn("original_name")
                    .AsString(255)
                    .NotNullable()

                .WithColumn("extension")
                    .AsString(20)
                    .NotNullable()

                .WithColumn("content_type")
                    .AsString(150)
                    .NotNullable()

                .WithColumn("size_bytes")
                    .AsInt64()
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

            Create.Index("ix_files_user_id")
                .OnTable("files").InSchema("storage")
                .OnColumn("user_id").Ascending();

            Create.ForeignKey("fk_files_user_id")
                .FromTable("files").InSchema("storage").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_files_user_id")
                .OnTable("files").InSchema("storage");

            Delete.Table("files").InSchema("storage");
        }
    }
}

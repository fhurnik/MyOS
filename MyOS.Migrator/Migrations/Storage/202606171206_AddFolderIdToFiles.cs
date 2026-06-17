using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    [Migration(202606171206)]
    public sealed class AddFolderIdToFiles : Migration
    {
        public override void Up()
        {
            Alter.Table("files").InSchema("storage")
                .AddColumn("folder_id").AsGuid().Nullable();

            Create.Index("ix_files_folder_id")
                .OnTable("files").InSchema("storage")
                .OnColumn("folder_id").Ascending();

            Create.ForeignKey("fk_files_folder_id")
                .FromTable("files").InSchema("storage").ForeignColumn("folder_id")
                .ToTable("folders").InSchema("storage").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_files_folder_id").OnTable("files").InSchema("storage");
            Delete.Index("ix_files_folder_id").OnTable("files").InSchema("storage");
            Delete.Column("folder_id").FromTable("files").InSchema("storage");
        }
    }
}

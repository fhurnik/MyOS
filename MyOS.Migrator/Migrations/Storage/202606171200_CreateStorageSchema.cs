using FluentMigrator;

namespace MyOS.Migrator.Migrations.Storage
{
    [Migration(202606171200)]
    public sealed class CreateStorageSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("storage");
        }

        public override void Down()
        {
            Delete.Schema("storage");
        }
    }
}

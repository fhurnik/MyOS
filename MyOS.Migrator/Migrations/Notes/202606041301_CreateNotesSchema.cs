using FluentMigrator;

namespace MyOS.Migrator.Migrations.Notes
{
    [Migration(202606041301)]
    public sealed class CreateNotesSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("notes");
        }

        public override void Down()
        {
            Delete.Schema("notes");
        }
    }
}

using FluentMigrator;

namespace MyOS.Migrator.Migrations.Identity
{
    [Migration(202604261255)]
    public class CreateIdentitySchema : Migration
    {
        public override void Down()
        {
            Delete.Schema("identity");
        }

        public override void Up()
        {
            Create.Schema("identity");
        }
    }
}

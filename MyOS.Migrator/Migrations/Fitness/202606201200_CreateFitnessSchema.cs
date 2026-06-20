using FluentMigrator;

namespace MyOS.Migrator.Migrations.Fitness
{
    [Migration(202606201200)]
    public sealed class CreateFitnessSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("fitness");
        }

        public override void Down()
        {
            Delete.Schema("fitness");
        }
    }
}

using FluentMigrator;

namespace MyOS.Migrator.Migrations.Fitness
{
    [Migration(202606201202)]
    public sealed class CreateWorkoutsTable : Migration
    {
        public override void Up()
        {
            Create.Table("workouts")
                .InSchema("fitness")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("date")
                    .AsDate()
                    .NotNullable()

                .WithColumn("notes")
                    .AsString(2000)
                    .Nullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("updated_at_utc")
                    .AsDateTime2()
                    .Nullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_workouts_user_id")
                .OnTable("workouts").InSchema("fitness")
                .OnColumn("user_id").Ascending();

            Create.ForeignKey("fk_workouts_user_id")
                .FromTable("workouts").InSchema("fitness").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_workouts_user_id").OnTable("workouts").InSchema("fitness");
            Delete.Table("workouts").InSchema("fitness");
        }
    }
}

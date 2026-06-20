using FluentMigrator;

namespace MyOS.Migrator.Migrations.Fitness
{
    [Migration(202606201201)]
    public sealed class CreateExercisesTable : Migration
    {
        public override void Up()
        {
            Create.Table("exercises")
                .InSchema("fitness")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("name")
                    .AsString(200)
                    .NotNullable()

                .WithColumn("activity_type")
                    .AsByte()
                    .NotNullable()

                .WithColumn("strength_category")
                    .AsByte()
                    .Nullable()

                .WithColumn("distance")
                    .AsInt32()
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

            Create.Index("ix_exercises_user_id")
                .OnTable("exercises").InSchema("fitness")
                .OnColumn("user_id").Ascending();

            Create.ForeignKey("fk_exercises_user_id")
                .FromTable("exercises").InSchema("fitness").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_exercises_user_id").OnTable("exercises").InSchema("fitness");
            Delete.Table("exercises").InSchema("fitness");
        }
    }
}

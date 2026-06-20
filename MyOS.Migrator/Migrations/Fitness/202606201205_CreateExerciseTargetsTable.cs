using FluentMigrator;

namespace MyOS.Migrator.Migrations.Fitness
{
    // One current target per exercise (unique exercise_id). No soft delete — replaced in place by PUT.
    [Migration(202606201205)]
    public sealed class CreateExerciseTargetsTable : Migration
    {
        public override void Up()
        {
            Create.Table("exercise_targets")
                .InSchema("fitness")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("exercise_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("user_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("value")
                    .AsDecimal(9, 2)
                    .NotNullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("updated_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ux_exercise_targets_exercise_id")
                .OnTable("exercise_targets").InSchema("fitness")
                .OnColumn("exercise_id").Ascending()
                .WithOptions().Unique();

            Create.Index("ix_exercise_targets_user_id")
                .OnTable("exercise_targets").InSchema("fitness")
                .OnColumn("user_id").Ascending();

            Create.ForeignKey("fk_exercise_targets_exercise_id")
                .FromTable("exercise_targets").InSchema("fitness").ForeignColumn("exercise_id")
                .ToTable("exercises").InSchema("fitness").PrimaryColumn("id");

            Create.ForeignKey("fk_exercise_targets_user_id")
                .FromTable("exercise_targets").InSchema("fitness").ForeignColumn("user_id")
                .ToTable("users").InSchema("identity").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_exercise_targets_user_id").OnTable("exercise_targets").InSchema("fitness");
            Delete.ForeignKey("fk_exercise_targets_exercise_id").OnTable("exercise_targets").InSchema("fitness");
            Delete.Table("exercise_targets").InSchema("fitness");
        }
    }
}

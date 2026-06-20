using FluentMigrator;

namespace MyOS.Migrator.Migrations.Fitness
{
    [Migration(202606201203)]
    public sealed class CreateWorkoutExercisesTable : Migration
    {
        public override void Up()
        {
            Create.Table("workout_exercises")
                .InSchema("fitness")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("workout_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("exercise_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("position")
                    .AsInt32()
                    .NotNullable()

                .WithColumn("duration")
                    .AsInt32()
                    .Nullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_workout_exercises_workout_id")
                .OnTable("workout_exercises").InSchema("fitness")
                .OnColumn("workout_id").Ascending();

            Create.Index("ix_workout_exercises_exercise_id")
                .OnTable("workout_exercises").InSchema("fitness")
                .OnColumn("exercise_id").Ascending();

            Create.ForeignKey("fk_workout_exercises_workout_id")
                .FromTable("workout_exercises").InSchema("fitness").ForeignColumn("workout_id")
                .ToTable("workouts").InSchema("fitness").PrimaryColumn("id");

            Create.ForeignKey("fk_workout_exercises_exercise_id")
                .FromTable("workout_exercises").InSchema("fitness").ForeignColumn("exercise_id")
                .ToTable("exercises").InSchema("fitness").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_workout_exercises_exercise_id").OnTable("workout_exercises").InSchema("fitness");
            Delete.ForeignKey("fk_workout_exercises_workout_id").OnTable("workout_exercises").InSchema("fitness");
            Delete.Table("workout_exercises").InSchema("fitness");
        }
    }
}

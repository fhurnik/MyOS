using FluentMigrator;

namespace MyOS.Migrator.Migrations.Fitness
{
    [Migration(202606201204)]
    public sealed class CreateExerciseSetsTable : Migration
    {
        public override void Up()
        {
            Create.Table("exercise_sets")
                .InSchema("fitness")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("workout_exercise_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("position")
                    .AsInt32()
                    .NotNullable()

                .WithColumn("reps")
                    .AsInt32()
                    .NotNullable()

                .WithColumn("weight")
                    .AsDecimal(5, 2)
                    .Nullable()

                .WithColumn("added_weight")
                    .AsDecimal(5, 2)
                    .Nullable()

                .WithColumn("negatives")
                    .AsInt32()
                    .Nullable()

                .WithColumn("rir")
                    .AsByte()
                    .Nullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.Index("ix_exercise_sets_workout_exercise_id")
                .OnTable("exercise_sets").InSchema("fitness")
                .OnColumn("workout_exercise_id").Ascending();

            Create.ForeignKey("fk_exercise_sets_workout_exercise_id")
                .FromTable("exercise_sets").InSchema("fitness").ForeignColumn("workout_exercise_id")
                .ToTable("workout_exercises").InSchema("fitness").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_exercise_sets_workout_exercise_id").OnTable("exercise_sets").InSchema("fitness");
            Delete.Table("exercise_sets").InSchema("fitness");
        }
    }
}

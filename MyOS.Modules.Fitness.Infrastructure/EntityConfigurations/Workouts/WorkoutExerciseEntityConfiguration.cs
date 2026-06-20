using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Workouts
{
    internal sealed class WorkoutExerciseEntityConfiguration : EntityConfiguration<WorkoutExercise>
    {
        public override void Configure(EntityTypeBuilder<WorkoutExercise> builder)
        {
            builder.ToTable("workout_exercises", "fitness");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.WorkoutId).HasColumnName("workout_id");
            builder.Property(x => x.ExerciseId).HasColumnName("exercise_id");
            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.Duration).HasColumnName("duration");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            // No navigation to Exercise — exercise_id is a scalar FK (enforced by migration);
            // the command side loads the Exercise separately when type/category is needed.
            builder.HasMany(x => x.Sets)
                .WithOne()
                .HasForeignKey(s => s.WorkoutExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => x.DeletedAtUtc == null);
            builder.HasIndex(x => x.WorkoutId).HasDatabaseName("IX_workout_exercises_workout_id");
            builder.HasIndex(x => x.ExerciseId).HasDatabaseName("IX_workout_exercises_exercise_id");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Workouts
{
    internal sealed class ExerciseSetEntityConfiguration : EntityConfiguration<ExerciseSet>
    {
        public override void Configure(EntityTypeBuilder<ExerciseSet> builder)
        {
            builder.ToTable("exercise_sets", "fitness");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.WorkoutExerciseId).HasColumnName("workout_exercise_id");
            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.Reps).HasColumnName("reps");
            builder.Property(x => x.Weight).HasColumnName("weight").HasColumnType("decimal(5,2)");
            builder.Property(x => x.AddedWeight).HasColumnName("added_weight").HasColumnType("decimal(5,2)");
            builder.Property(x => x.Negatives).HasColumnName("negatives");
            builder.Property(x => x.Rir).HasColumnName("rir");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasQueryFilter(x => x.DeletedAtUtc == null);
            builder.HasIndex(x => x.WorkoutExerciseId).HasDatabaseName("IX_exercise_sets_workout_exercise_id");
        }
    }
}

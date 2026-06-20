using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Exercises
{
    internal sealed class ExerciseEntityConfiguration : EntityConfiguration<Exercise>
    {
        public override void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ToTable("exercises", "fitness");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
            builder.Property(x => x.ActivityType).HasColumnName("activity_type");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasDiscriminator(x => x.ActivityType)
                .HasValue<CardioExercise>(ActivityType.Cardio)
                .HasValue<StrengthExercise>(ActivityType.Strength);

            builder.HasQueryFilter(x => x.DeletedAtUtc == null);

            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_exercises_user_id");
        }
    }
}

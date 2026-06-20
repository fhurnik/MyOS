using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Fitness.Domain.Targets;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Targets
{
    internal sealed class ExerciseTargetEntityConfiguration : EntityConfiguration<ExerciseTarget>
    {
        public override void Configure(EntityTypeBuilder<ExerciseTarget> builder)
        {
            builder.ToTable("exercise_targets", "fitness");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ExerciseId).HasColumnName("exercise_id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Value).HasColumnName("value").HasColumnType("decimal(9,2)");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

            builder.HasIndex(x => x.ExerciseId).IsUnique().HasDatabaseName("UX_exercise_targets_exercise_id");
        }
    }
}

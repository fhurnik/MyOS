using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Workouts
{
    internal sealed class WorkoutEntityConfiguration : EntityConfiguration<Workout>
    {
        public override void Configure(EntityTypeBuilder<Workout> builder)
        {
            builder.ToTable("workouts", "fitness");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Date).HasColumnName("date");
            builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(2000);
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");

            builder.HasMany(x => x.Exercises)
                .WithOne()
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => x.DeletedAtUtc == null);
            builder.HasIndex(x => x.UserId).HasDatabaseName("IX_workouts_user_id");
        }
    }
}

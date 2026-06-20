using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Exercises
{
    internal sealed class StrengthExerciseEntityConfiguration : IEntityTypeConfiguration<StrengthExercise>
    {
        public void Configure(EntityTypeBuilder<StrengthExercise> builder)
        {
            builder.Property(x => x.StrengthCategory).HasColumnName("strength_category");
        }
    }
}

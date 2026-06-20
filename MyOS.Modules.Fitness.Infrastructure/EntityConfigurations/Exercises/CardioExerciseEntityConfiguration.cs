using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Infrastructure.EntityConfigurations.Exercises
{
    internal sealed class CardioExerciseEntityConfiguration : IEntityTypeConfiguration<CardioExercise>
    {
        public void Configure(EntityTypeBuilder<CardioExercise> builder)
        {
            builder.Property(x => x.Distance).HasColumnName("distance");
        }
    }
}

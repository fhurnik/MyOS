using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Application.Exercises.Shared
{
    public sealed record ExerciseDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public ActivityType ActivityType { get; init; }
        public StrengthCategory? StrengthCategory { get; init; }
        public int? Distance { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}

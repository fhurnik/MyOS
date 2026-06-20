namespace MyOS.Modules.Fitness.Application.Workouts.Shared
{
    public sealed record ExerciseSetDto
    {
        public Guid Id { get; init; }
        public Guid WorkoutExerciseId { get; init; }
        public int Position { get; init; }
        public int Reps { get; init; }
        public decimal? Weight { get; init; }
        public decimal? AddedWeight { get; init; }
        public int? Negatives { get; init; }
        public byte? Rir { get; init; }
    }
}

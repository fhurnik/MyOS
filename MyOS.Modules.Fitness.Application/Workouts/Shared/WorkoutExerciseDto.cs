using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Application.Workouts.Shared
{
    // Flat fields are read from v_workout_exercises by Dapper; Sets is assembled in the handler.
    public sealed record WorkoutExerciseDto
    {
        public Guid Id { get; init; }
        public Guid WorkoutId { get; init; }
        public Guid ExerciseId { get; init; }
        public string ExerciseName { get; init; } = string.Empty;
        public ActivityType ActivityType { get; init; }
        public StrengthCategory? StrengthCategory { get; init; }
        public int Position { get; init; }
        public int? Duration { get; init; }
        public IReadOnlyList<ExerciseSetDto> Sets { get; init; } = [];
    }
}

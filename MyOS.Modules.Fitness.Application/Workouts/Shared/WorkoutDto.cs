namespace MyOS.Modules.Fitness.Application.Workouts.Shared
{
    // Assembled in the handler from the summary + exercises + sets — positional is fine here.
    public sealed record WorkoutDto(
        Guid Id,
        Guid UserId,
        DateTime Date,
        string? Notes,
        IReadOnlyList<WorkoutExerciseDto> Exercises,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}

namespace MyOS.Modules.Fitness.Application.Stats.Shared
{
    // Progression series for one exercise plus its current target (null when none set) — one chart.
    public sealed record ProgressionDto(
        Guid ExerciseId,
        decimal? TargetValue,
        IReadOnlyList<ProgressionPointDto> Points);
}

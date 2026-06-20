namespace MyOS.Modules.Fitness.Application.Workouts.Shared
{
    public sealed record WorkoutSummaryDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public DateTime Date { get; init; }
        public string? Notes { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
